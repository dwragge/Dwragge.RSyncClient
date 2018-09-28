using System;
using System.Threading;
using System.Threading.Tasks;
using Dwragge.BlobBlaze.Entities;
using MediatR;
using Quartz;

namespace Dwragge.BlobBlaze.Application.Requests
{
    public class SyncFolderNowRequest : IRequest
    {
        public SyncFolderNowRequest(BackupFolder folder)
        {
            Folder = folder;
        }
        public BackupFolder Folder { get; }
    }

    public class SyncFolderNowRequestHandler : IRequestHandler<SyncFolderNowRequest>
    {
        private readonly IScheduler _scheduler;

        public SyncFolderNowRequestHandler(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public async Task<Unit> Handle(SyncFolderNowRequest request, CancellationToken cancellationToken)
        {
            var folder = request.Folder;
            var jobKey = JobKey.Create(folder.BackupFolderId.ToString(), "discover-files");

            if (!await _scheduler.CheckExists(jobKey, cancellationToken))
            {
                throw new InvalidOperationException("Job must be already scheduled before trying to run now");
            }

            await _scheduler.TriggerJob(jobKey, cancellationToken);
            return Unit.Value;
        }
    }
}
