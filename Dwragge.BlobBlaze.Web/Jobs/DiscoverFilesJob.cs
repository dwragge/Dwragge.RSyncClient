using Dwragge.BlobBlaze.Application;
using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dwragge.BlobBlaze.Web.Jobs
{
    public class DiscoverFilesJob : IJob
    {
        private readonly ILogger _logger;
        private readonly IApplicationContextFactory _contextFactory;
        private readonly IDirectoryEnumerator _directoryEnumerator;
        private readonly IUploadProcessor _uploadProcessor;

        public BackupFolder Folder { get; set; }

        public async Task Execute(IJobExecutionContext jobContext)
        {
            using (var scope = _logger.BeginScope("Executing Check Files Job, fired at {date} for folder {path}", jobContext.FireTimeUtc.ToLocalTime(), Folder.Path))
            {
                var job = new BackupFolderJob(Folder);
                try
                {
                    Folder = (BackupFolder)jobContext.MergedJobDataMap["Folder"] ?? throw new ArgumentNullException(nameof(Folder));

                    using (var context = _contextFactory.CreateContext())
                    {
                        // check if already running a job for this folder and return if so
                        if (context.BackupJobs.Any(j => j.BackupFolderId == Folder.BackupFolderId && 
                                (j.Status == BackupFolderJobStatus.InProgress || j.Status == BackupFolderJobStatus.Pending)))
                        {
                            return;
                        }

                        context.BackupJobs.Add(job);
                        await context.SaveChangesAsync(jobContext.CancellationToken);
                    }

                    var files = await GetFilesToTransfer();
                    QueueFilesAsPending(files, job);
                }
                catch (Exception ex)
                {
                    using (var context = _contextFactory.CreateContext())
                    {
                        job.Status = BackupFolderJobStatus.Errored;
                        await context.SaveChangesAsync();
                    }

                    _logger.LogCritical(ex, "Exception while running job: {message}. {innerException} \n {stackTrace}", ex.Message, ex.InnerException?.Message, ex.StackTrace);
                    throw;
                }
            }
        }

        private async Task<IReadOnlyCollection<string>> GetFilesToTransfer()
        {
            var allFiles = await _directoryEnumerator.GetFiles(Folder.Path);

            var newFiles = new HashSet<string>();
            var potentiallyModifiedFiles = new List<TrackedFile>();

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

            var modifiedFiles = potentiallyModifiedFiles.Where(f => f.IsOutOfDate);
            var ret = modifiedFiles.Select(t => t.FileName).ToList().Union(newFiles);
            return ret.ToList().AsReadOnly();
        }

        private void QueueFilesAsPending(IReadOnlyCollection<string> files, BackupFolderJob job)
        {
            foreach (var file in files)
            {
                var fileJob = new BackupFileUploadJob(job, new System.IO.FileInfo(file));
                _uploadProcessor.AddJob(fileJob);
            }
        }
    }
}
