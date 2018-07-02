using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.WindowsService.Jobs;
using Quartz;

namespace Dwragge.RCloneClient.WindowsService
{
    public class QuartzJobFactory
    {
        public static (IJobDetail Job, ITrigger Trigger) CreateSyncJob(BackupFolderInfo info)
        {
            var job = JobBuilder.Create<PreCheckMoveFilesJob>()
                .WithIdentity(info.BackupFolderId.ToString(), "sync")
                .Build();
            job.JobDataMap["Folder"] = info;

            var trigger = TriggerBuilder.Create()
                .ForJob(job)
                .StartNow()
                //.WithCronSchedule($"0 {info.SyncTime.Minute} {info.SyncTime.Hour} 1/{info.SyncTimeSpan.Days} * ?")
                .Build();

            return (job, trigger);
        }
    }
}
