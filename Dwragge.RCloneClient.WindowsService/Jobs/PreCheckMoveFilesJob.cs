using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NLog;
using Quartz;
using IsolationLevel = System.Data.IsolationLevel;

namespace Dwragge.RCloneClient.WindowsService.Jobs
{
    public class PreCheckMoveFilesJob : IJob
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IJobContextFactory _contextFactory;
        private readonly IBackedUpFileTracker _tracker;

        public BackupFolderInfo Folder { get; set; }

        public PreCheckMoveFilesJob(IJobContextFactory contextFactory, IBackedUpFileTracker tracker)
        {
            _contextFactory = contextFactory;
            _tracker = tracker;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Folder = (BackupFolderInfo)context.MergedJobDataMap["Folder"] ?? throw new ArgumentNullException(nameof(Folder), "Command was not set");
            

            try
            {
                var files = await GetFilesToTransfer();
                QueueFilesAsPending(files);
                // mark files as not archived
                //throw new NotImplementedException();
                //transaction.Complete();
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception while running job: {ex.Message}");
                throw;
            }
        }

        private async Task<IEnumerable<string>> GetFilesToTransfer()
        {
            var enumerator = new DirectoryEnumerator();
            var allFiles = await enumerator.GetFiles(Folder.Path);

            var newFiles = new List<string>();
            var potentiallyModifiedFiles = new List<TrackedFileDto>();

            using (var context = _contextFactory.CreateContext())
            {
                var trackedFiles = await context.TrackedFiles.Where(t => t.BackupFolderId == Folder.Id).ToDictionaryAsync(t => t.FileName);
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
            newFiles.AddRange(modifiedFiles.Select(t => t.FileName));
            return newFiles;
        }

        private IEnumerable<TrackedFileDto> GetModifiedFiles(IEnumerable<TrackedFileDto> potentiallyModifiedFiles)
        {
            return potentiallyModifiedFiles.AsParallel().Where(file =>
            {
                var onDiskFile = new FileInfo(file.FileName);
                if (!DateTimesEqualToSeconds(file.LastModified, onDiskFile.LastWriteTimeUtc)
                    || file.SizeBytes != onDiskFile.Length)
                {
                    return true;
                }

                return false;
            });
        }

        private bool DateTimesEqualToSeconds(DateTime d1, DateTime d2)
        {
            if (d1.Kind != d2.Kind) throw new ArgumentException("DateTimes must be same type");
            var a = new DateTime(d1.Year, d1.Month, d1.Day, d1.Hour, d1.Minute, d1.Second, d1.Kind);
            var b = new DateTime(d2.Year, d2.Month, d2.Day, d2.Hour, d2.Minute, d2.Second, d2.Kind);

            return DateTime.Compare(a, b) == 0;
        }

        private void QueueFilesAsPending(IEnumerable<string> newFiles)
        {
            IEnumerable<string> files = newFiles as string[] ?? newFiles.ToArray();

            _logger.Info($"Enqueuing {files.Count()} files to be transferred");
            var dtos = files.Select(f => new PendingFileDto
            {
                FileName = f,
                BackupFolderId = Folder.Id
            });

            using (var context = _contextFactory.CreateContext(false))
            {
                using (var trans = context.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        var alreadyInQueue = context.PendingFiles.Where(t => files.Contains(t.FileName));
                        if (alreadyInQueue.Any())
                        {
                            _logger.Info($"Overwriting {alreadyInQueue.Count()} pending items that were already in the queue"));
                            context.PendingFiles.RemoveRange(alreadyInQueue);
                        };

                        context.PendingFiles.AddRangeAsync(dtos);
                        context.SaveChangesAsync();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to queue new files: {ex.Message}");
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
