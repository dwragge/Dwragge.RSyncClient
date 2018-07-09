using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.Persistence;
using Microsoft.EntityFrameworkCore;
using NLog;
using Quartz;

namespace Dwragge.RCloneClient.WindowsService.Jobs
{
    public class PreCheckMoveFilesJob : IJob
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IJobContextFactory _contextFactory;

        public BackupFolderInfo Folder { get; set; }

        public PreCheckMoveFilesJob(IJobContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Folder = (BackupFolderInfo)context.MergedJobDataMap["Folder"] ?? throw new ArgumentNullException(nameof(Folder), "Command was not set");
            _logger.Info($"Executing Check Files Job, fired at {context.FireTimeUtc.ToLocalTime()} for folder {Folder.Path}");

            try
            {
                var files = await GetFilesToTransfer();
                _logger.Info($"Found {files.Count} files that need to be transferred.");

                QueueFilesAsPending(files);
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception while running job: {ex.Message}");
                throw;
            }
        }

        private async Task<ImmutableHashSet<string>> GetFilesToTransfer()
        {
            var enumerator = new DirectoryEnumerator();
            var allFiles = await enumerator.GetFiles(Folder.Path);

            var newFiles = new HashSet<string>();
            var potentiallyModifiedFiles = new List<TrackedFileDto>();

            using (var context = _contextFactory.CreateContext())
            {
                var trackedFiles = await context.TrackedFiles.Where(t => t.BackupFolderId == Folder.BackupFolderId).ToDictionaryAsync(t => t.FileName);
                foreach (var file in allFiles)
                {
                    if (trackedFiles.ContainsKey(file))
                    {
                        potentiallyModifiedFiles.Add(trackedFiles[file]);
                    }
                    else
                    {
                        newFiles.Add(file);
                    }
                }
            }

            var modifiedFiles = GetModifiedFiles(potentiallyModifiedFiles);
            var ret = modifiedFiles.Select(t => t.FileName).ToImmutableHashSet().Union(newFiles);
            return ret;
        }

        private IEnumerable<TrackedFileDto> GetModifiedFiles(IEnumerable<TrackedFileDto> potentiallyModifiedFiles)
        {
            return potentiallyModifiedFiles.AsParallel().Where(file =>
            {
                var onDiskFile = new FileInfo(file.FileName);
                if (!DateTimesEqualToSeconds(file.LastModified, onDiskFile.LastWriteTimeUtc)
                    || file.SizeBytes != onDiskFile.Length)
                {
                    _logger.Debug($"File {file.FileName} has changed. Tracked Last Modified: {file.LastModified:G} FileSystem Last Modified: {onDiskFile.LastWriteTime:G}. Tracked Size: {file.SizeBytes} File System Size: {onDiskFile.Length}");
                    return true;
                }

                return false;
            });
        }

        private bool DateTimesEqualToSeconds(DateTime d1, DateTime d2)
        {
            var a = new DateTime(d1.Year, d1.Month, d1.Day, d1.Hour, d1.Minute, d1.Second, d1.Kind);
            var b = new DateTime(d2.Year, d2.Month, d2.Day, d2.Hour, d2.Minute, d2.Second, d2.Kind);

            return DateTime.Compare(a, b) == 0;
        }

        private void QueueFilesAsPending(ICollection<string> files)
        {
            _logger.Info($"Enqueuing {files.Count} files to be transferred");
            var dtos = files.Select(f => new PendingFileDto
            {
                FileName = f,
                BackupFolderId = Folder.BackupFolderId,
                QueuedTime = DateTime.Now
            });

            using (var context = _contextFactory.CreateContext(false))
            {
                try
                {
                    var alreadyInQueue = context.PendingFiles.Where(t => t.BackupFolderId == Folder.BackupFolderId).Select(t => t.FileName).ToImmutableList();
                    if (alreadyInQueue.Any()) _logger.Info($"Found {alreadyInQueue.Count} items already in queue... skipping");
                    foreach (var queued in alreadyInQueue)
                    {
                        files.Remove(queued);
                    }

                    context.PendingFiles.AddRangeAsync(dtos);
                    context.BackupFolders.Find(Folder.BackupFolderId).LastSync = DateTime.Now;
                    context.SaveChangesAsync();
                    _logger.Info($"Successfully queued {files.Count} new files.");
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to queue new files: {ex.Message}");
                    throw;
                }   
            }
        }
    }
}
