using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Dwragge.BlobBlaze.Application.Jobs
{
    public class DiscoverFilesJob : IJob
    {
        private readonly ILogger _logger;
        private readonly IApplicationContextFactory _contextFactory;
        private readonly IDirectoryEnumerator _directoryEnumerator;
        private readonly IUploadProcessor _uploadProcessor;

        public BackupFolder Folder { get; set; }

        public static string JobGroupName => "discover-files";

        public DiscoverFilesJob(ILogger<DiscoverFilesJob> logger, IApplicationContextFactory contextFactory,
                            IDirectoryEnumerator enumerator, IUploadProcessor uploadProcessor)
        {
            _logger = logger;
            _contextFactory = contextFactory;
            _directoryEnumerator = enumerator;
            _uploadProcessor = uploadProcessor;
        }

        public async Task Execute(IJobExecutionContext jobContext)
        {
            Folder = (BackupFolder)jobContext.MergedJobDataMap["Folder"] ?? throw new ArgumentNullException(nameof(Folder));
            _logger.LogInformation("Executing Check Files Job, fired at {date} for folder {path}", jobContext.FireTimeUtc.ToLocalTime(), Folder.Path);
            var job = new BackupFolderJob(Folder);
            try
            {
                using (var context = _contextFactory.CreateContext())
                {
                    // check if already running a job for this folder and return if so
                    if (context.BackupJobs.Any(j => j.BackupFolderId == Folder.BackupFolderId &&
                            (j.Status == BackupFolderJobStatus.InProgress || j.Status == BackupFolderJobStatus.Pending)))
                    {
                        _logger.LogInformation("Job found already in progress, skipping...");
                        return;
                    }

                    var files = await GetFilesToTransfer();
                    context.Entry(job.Folder).State = EntityState.Unchanged;
                    job.NumFiles = files.Count;

                    context.Attach(Folder);
                    Folder.LastSync = DateTime.UtcNow;

                    await context.BackupJobs.AddAsync(job);
                    await context.SaveChangesAsync(jobContext.CancellationToken);

                    QueueFilesAsPending(files, job);
                }
            }
            catch (Exception ex)
            {
                using (var context = _contextFactory.CreateContext())
                {
                    context.BackupJobs.Attach(job);
                    job.Status = BackupFolderJobStatus.Errored;
                    await context.SaveChangesAsync();
                }

                _logger.LogCritical(ex, "Exception while running job: {message}. {innerException} \n {stackTrace}", ex.Message, ex.InnerException?.Message, ex.StackTrace);
                throw;
            }
        }

        private async Task<IReadOnlyCollection<string>> GetFilesToTransfer()
        {
            var allFiles = await _directoryEnumerator.GetFiles(Folder.Path);

            var newFiles = new HashSet<string>();
            var potentiallyModifiedFiles = new List<TrackedFile>();

            using (var context = _contextFactory.CreateContext())
            {
                var trackedFiles = await context.TrackedFiles.AsNoTracking()
                    .Where(t => t.BackupFolderId == Folder.BackupFolderId)
                    .ToDictionaryAsync(t => t.FileName);

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
