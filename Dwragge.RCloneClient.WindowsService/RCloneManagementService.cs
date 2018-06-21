using Quartz;

namespace Dwragge.RCloneClient.WindowsService
{
    public class RCloneManagementService : IRCloneManagementService
    {
        private readonly IScheduler _scheduler;

        public RCloneManagementService(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public string HelloWorld()
        {
            return "Hello, World";
        }

        public void PostHelloJob(string name)
        {
            var job = JobBuilder.Create<HelloJob>()
                .WithIdentity("job1", "group1")
                .UsingJobData("Name", name)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1")
                .ForJob(job)
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(10).RepeatForever())
                .Build();

            _scheduler.ScheduleJob(job, trigger);

        }
    }
}
