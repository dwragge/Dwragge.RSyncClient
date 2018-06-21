using Quartz;
using Quartz.Impl;

namespace Dwragge.RSyncClient.WindowsService
{
    class QuartzSchedulerFactory
    {
        public static IScheduler CreateQuartzScheduler()
        {
            var schedulerFactory = new StdSchedulerFactory();
            var scheduler = schedulerFactory.GetScheduler().Result;

            return scheduler;
        }
    }
}
