using System.Collections.Generic;
using Dwragge.BlobBlaze.Entities;

namespace Dwragge.BlobBlaze.Application
{
    public interface IUploadProcessor
    {
        IReadOnlyCollection<BackupFileUploadJob> FailedJobs { get; }

        void AddJob(BackupFileUploadJob job);
        void NotifyOfPendingTasks();
    }
}