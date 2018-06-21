using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac;
using Dwragge.RCloneClient.Common;
using Quartz;
using Topshelf;
using Autofac.Integration.Wcf;

namespace Dwragge.RCloneClient.WindowsService
{
    public class ServiceWindowsService : ServiceControl
    {
        public ServiceHost ServiceHost;
        private IScheduler _scheduler;
        
        public bool Start(HostControl hostControl)
        {
            _scheduler = QuartzSchedulerFactory.CreateQuartzScheduler();
            _scheduler.Start().Wait();

            InitializeServiceHost();

            return true;
        }

        private void InitializeServiceHost()
        {
            ServiceHost?.Close();

            const string baseAddress = "net.pipe://localhost/com.Dwragge.RCloneClientService";
            ServiceHost = new ServiceHost(typeof(RCloneManagementService), new Uri(baseAddress));
            ServiceHost.AddServiceEndpoint(typeof(IRCloneManagementService), new NetNamedPipeBinding(), baseAddress);

            var behaviour = new ServiceMetadataBehavior();
            ServiceHost.Description.Behaviors.Add(behaviour);
            ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexNamedPipeBinding(), baseAddress + "/mex/");

            var container = BuildContainer();
            ServiceHost.AddDependencyInjectionBehavior<IRCloneManagementService>(container);

            ServiceHost.Open();
        }

        private IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_scheduler);
            builder.RegisterType<RCloneManagementService>().As<IRCloneManagementService>();

            return builder.Build();
        }

        public bool Stop(HostControl hostControl)
        {
            ServiceHost?.Close();
            ServiceHost = null;

            if (DebugChecker.IsDebug)
            {
                _scheduler.Shutdown(false).Wait(); 
            }
            else
            {
                _scheduler.Shutdown(true).Wait();
            }
            return true;
        }
    }
}
