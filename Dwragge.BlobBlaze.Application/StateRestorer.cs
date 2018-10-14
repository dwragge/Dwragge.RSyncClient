using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dwragge.BlobBlaze.Application.Jobs;
using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;

namespace Dwragge.BlobBlaze.Application
{
    public class StateRestorer : IStateRestorer
    {
        private readonly ILogger _logger;
        private readonly IScheduler _scheduler;
        private readonly ApplicationContext _context;

        public StateRestorer(ILogger<StateRestorer> logger, IApplicationContextFactory contextFactory, IScheduler scheduler)
        {
            _logger = logger;
            _scheduler = scheduler;
            _context = contextFactory.CreateContext();
        }

        public void RestoreState()
        {
            _logger.LogInformation("Beginning Restoring State");
            var jobs = _context.BackupJobs.Where(j =>
                j.Status == BackupFolderJobStatus.InProgress || j.Status == BackupFolderJobStatus.Pending).ToList();
            _context.BackupJobs.RemoveRange(jobs);
            _context.SaveChanges();

            var jobkeys = _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupContains(DiscoverFilesJob.JobGroupName))
                .Result.Where(jobKey => jobs.Select(j => j.BackupFolderId.ToString()).Contains(jobKey.Name));
            foreach (var jobkey in jobkeys)
            {
                _scheduler.TriggerJob(jobkey).Wait();
            }
        }

        private void HandleInProgressJobs()
        {
            var folderJobs = _context.BackupJobs.Where(j => j.Status == BackupFolderJobStatus.InProgress).ToList();
            _logger.LogInformation("Found {count} jobs that were in progress", folderJobs.Count());

            //foreach (var job in folderJobs)
            //{
            //    var uploadJobs = _context.UploadJobs
            //        .Include(j => j.ParentJob)
            //        .Include(j => j.ParentJob.Folder)
            //        .Where(j => j.ParentJobId == job.BackupFolderJobId &&
            //                    j.Status == BackupFileUploadJobStatus.InProgress ||
            //                    j.Status == BackupFileUploadJobStatus.ErroredRetrying ||
            //                    j.Status == BackupFileUploadJobStatus.Pending);
            //    // if there's no uploads left, the job as a whole must be finished, or at least in a finished state
            //    if (!uploadJobs.Any())
            //    {
            //        _context.BackupJobs.Remove(job);
            //    }
            //    else
            //    {
            //        foreach (var uploadJob in uploadJobs)
            //        {
            //            _processor.AddJob(uploadJob);
            //        }
            //    }
            //}
        }

        private void HandlePendingJobs()
        {
            var folderJobs = _context.BackupJobs.Where(j => j.Status == BackupFolderJobStatus.Pending).ToList();
            _context.BackupJobs.RemoveRange(folderJobs);
            _context.SaveChanges();
            _logger.LogInformation("Removed {n} jobs that were Pending.", folderJobs.Count);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
