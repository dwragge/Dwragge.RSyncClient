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
            var job = CreateJob(folder, folder.BackupFolderId.ToString());
            
            var triggerCron =
                new CronExpression(
                    $"0 {folder.SyncTime.Minute} {folder.SyncTime.Hour} */{folder.SyncTimeSpan.Days} * ?");
            var trigger = TriggerBuilder.Create()
                .ForJob(job)
                .StartNow()
                .WithCronSchedule(triggerCron.CronExpressionString)
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
            _logger.LogInformation($"Created sync job. Name = {folder.Name}, Path = {folder.Path}, Id = {folder.BackupFolderId}, Next Fire Time {trigger.GetNextFireTimeUtc()?.ToLocalTime()}");

            if (trigger.GetNextFireTimeUtc()?.DateTime - DateTime.UtcNow > TimeSpan.FromHours(24))
            {
                var triggerToday = TriggerBuilder.Create()
                    .ForJob(job)
                    .StartAt(DateTimeOffset.Parse($"{folder.SyncTime.Hour}:{folder.SyncTime.Minute}").AddDays(1))
                    .Build();

                await _scheduler.ScheduleJob(triggerToday, cancellationToken);
                _logger.LogInformation($"Created additional sync job for today. Fire time {triggerToday.GetNextFireTimeUtc()?.ToLocalTime()}");
            }

            return Unit.Value;
        }

        public IJobDetail CreateJob(BackupFolder folder, string id)
        {
            var job = JobBuilder.Create<DiscoverFilesJob>()
                .WithIdentity(id, DiscoverFilesJob.JobGroupName)
                .Build();
            job.JobDataMap["Folder"] = folder;
            return job;
        }
    }
}
