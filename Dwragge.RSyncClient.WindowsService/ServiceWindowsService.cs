using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Dwragge.RSyncClient.WindowsService;
using Quartz;
using Topshelf;

namespace Dwragge.RCloneClient.WindowsService
{
    public class ServiceWindowsService : ServiceControl
    {
        public ServiceHost ServiceHost;
        private IScheduler _scheduler;
        
        public bool Start(HostControl hostControl)
        {
            InitializeServiceHost();

            _scheduler = QuartzSchedulerFactory.CreateQuartzScheduler();
            _scheduler.Start().Wait();

            var job = JobBuilder.Create<HelloJob>().WithIdentity("job1", "group1").Build();
            var trigger = TriggerBuilder.Create().WithIdentity("trigger1").StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(10)).Build();

            _scheduler.ScheduleJob(job, trigger);

            return true;
        }

        private void InitializeServiceHost()
        {
            ServiceHost?.Close();

            var baseAddress = "net.pipe://localhost/com.Dwragge.RsyncClientService";
            ServiceHost = new ServiceHost(typeof(Service), new Uri(baseAddress));
            ServiceHost.AddServiceEndpoint(typeof(IService), new NetNamedPipeBinding(), baseAddress);

            var behaviour = new ServiceMetadataBehavior();
            ServiceHost.Description.Behaviors.Add(behaviour);
            ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexNamedPipeBinding(), baseAddress + "/mex/");

            ServiceHost.Open();
        }

        public bool Stop(HostControl hostControl)
        {
            ServiceHost?.Close();
            ServiceHost = null;

            _scheduler.Shutdown().Wait();
            return true;
        }
    }
}
