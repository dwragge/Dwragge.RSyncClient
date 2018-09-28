using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dwragge.BlobBlaze.Application
{
    public class StateRestorer : IStateRestorer
    {
        private readonly ILogger _logger;
        private readonly IUploadProcessor _processor;
        private readonly ApplicationContext _context;

        public StateRestorer(ILogger<StateRestorer> logger, IApplicationContextFactory contextFactory, IUploadProcessor processor)
        {
            _logger = logger;
            _processor = processor;
            _context = contextFactory.CreateContext();
        }

        public void RestoreState()
        {
            _logger.LogInformation("Beginning Restoring State");
            HandleInProgressJobs();
            HandlePendingJobs();
        }

        private void HandleInProgressJobs()
        {
            var folderJobs = _context.BackupJobs.Where(j => j.Status == BackupFolderJobStatus.InProgress).ToList();
            _logger.LogInformation("Found {count} jobs that were in progress", folderJobs.Count());
            foreach (var job in folderJobs)
            {
                var uploadJobs = _context.UploadJobs
                    .Include(j => j.ParentJob)
                    .Include(j => j.ParentJob.Folder)
                    .Where(j => j.ParentJobId == job.BackupFolderJobId &&
                                j.Status == BackupFileUploadJobStatus.InProgress ||
                                j.Status == BackupFileUploadJobStatus.ErroredRetrying ||
                                j.Status == BackupFileUploadJobStatus.Pending);
                // if there's no uploads left, the job as a whole must be finished, or at least in a finished state
                if (!uploadJobs.Any())
                {
                    _context.BackupJobs.Remove(job);
                }
                else
                {
                    foreach (var uploadJob in uploadJobs)
                    {
                        _processor.AddJob(uploadJob);
                    }
                }
            }
        }

        private void HandlePendingJobs()
        {
            var folderJobs = _context.BackupJobs.Where(j => j.Status == BackupFolderJobStatus.Pending);
            foreach (var folderJob in folderJobs)
            {

            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
