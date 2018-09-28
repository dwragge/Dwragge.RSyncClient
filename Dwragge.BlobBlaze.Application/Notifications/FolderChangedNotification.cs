using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dwragge.BlobBlaze.Application.Requests;
using Dwragge.BlobBlaze.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Dwragge.BlobBlaze.Application.Notifications
{
    public class FolderChangedNotification : INotification
    {
        public FolderChangedNotification(BackupFolder folder)
        {
            Folder = folder;
        }

        public BackupFolder Folder { get; }
    }

    public class ChangeScheduleTimeIfNeeded : INotificationHandler<FolderChangedNotification>
    {
        private readonly IScheduler _scheduler;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public ChangeScheduleTimeIfNeeded(IScheduler scheduler, IMediator mediator, ILogger<ChangeScheduleTimeIfNeeded> logger)
        {
            _scheduler = scheduler;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Handle(FolderChangedNotification notification, CancellationToken cancellationToken)
        {
            var folder = notification.Folder;
            _logger.LogInformation("Folder {name} was changed, rescheduling if SyncTime was changed", folder.Name);

            var jobKey = JobKey.Create(folder.BackupFolderId.ToString(), "discover-files");
            var triggers = await _scheduler.GetTriggersOfJob(jobKey, cancellationToken);
            var trigger = triggers.SingleOrDefault() ?? throw new InvalidOperationException("Must only have one trigger");

            var nextTime = trigger.GetNextFireTimeUtc();
            if (nextTime.HasValue)
            {
                var time = nextTime.Value;
                var currentTimeValue = new TimeValue(time.Hour, time.Minute);
                if (folder.SyncTime == currentTimeValue)
                {
                    // don't reschedule if we don't have to
                    return;
                }
            }

            await _scheduler.DeleteJob(jobKey, cancellationToken);
            await _mediator.Send(new ScheduleJobForFolderRequest(folder), cancellationToken);
        }
    }
}
