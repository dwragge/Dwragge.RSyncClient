using System;
using System.Threading;
using System.Threading.Tasks;
using Dwragge.BlobBlaze.Application.Jobs;
using Dwragge.BlobBlaze.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Dwragge.BlobBlaze.Application.Requests
{
    public class ScheduleJobForFolderRequest : IRequest
    {
        public ScheduleJobForFolderRequest(BackupFolder folder)
        {
            Folder = folder;
        }
        public BackupFolder Folder { get; set; }
    }

    public class ScheduleJobForFolderRequestHandler : IRequestHandler<ScheduleJobForFolderRequest>
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger _logger;

        public ScheduleJobForFolderRequestHandler(IScheduler scheduler, ILogger<ScheduleJobForFolderRequestHandler> logger)
        {
            _scheduler = scheduler;
            _logger = logger;
        }

        public async Task<Unit> Handle(ScheduleJobForFolderRequest request, CancellationToken cancellationToken)
        {
            var folder = request.Folder;

            var job = JobBuilder.Create<DiscoverFilesJob>()
                .WithIdentity(folder.BackupFolderId.ToString(), "discover-files")
                .Build();
            job.JobDataMap["Folder"] = folder;

            var triggerCron =
                new CronExpression(
                    $"0 {folder.SyncTime.Minute} {folder.SyncTime.Hour} */{folder.SyncTimeSpan.Days} * ?")
            var startTimeUtc = DateTime.Parse(folder.SyncTime.ToString()).AddDays(1).ToUniversalTime();
            var trigger = TriggerBuilder.Create()
                .ForJob(job)
                .StartAt(startTimeUtc)
                .WithCronSchedule(triggerCron.CronExpressionString)
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
            _logger.LogInformation($"Created sync job. Name = {folder.Name}, Path = {folder.Path}, Id = {folder.BackupFolderId}, Next Fire Time {trigger.GetNextFireTimeUtc()?.ToLocalTime()}");
            return Unit.Value;
        }
    }

}
