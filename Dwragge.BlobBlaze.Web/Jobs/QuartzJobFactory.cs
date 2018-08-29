using Dwragge.BlobBlaze.Entities;
using Quartz;

namespace Dwragge.BlobBlaze.Web.Jobs
{
    public class QuartzJobFactory
    {
        public static (IJobDetail Job, ITrigger Trigger) CreateSyncJob(BackupFolder info)
        {
            var job = JobBuilder.Create<DiscoverFilesJob>()
                .WithIdentity(info.BackupFolderId.ToString(), "discover-files")
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
