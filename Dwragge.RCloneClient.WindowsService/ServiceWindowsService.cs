using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;
using Autofac;
using Dwragge.RCloneClient.Common;
using Quartz;
using Topshelf;
using Autofac.Integration.Wcf;
using Dwragge.RCloneClient.Persistence;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Dwragge.RCloneClient.WindowsService
{
    public class ServiceWindowsService : ServiceControl
    {
        public ServiceHost ServiceHost;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IScheduler _scheduler;
        
        public bool Start(HostControl hostControl)
        {
            var startupTasks = new List<Task>
            {
                Task.Run(async () =>
                {
                    _scheduler = QuartzSchedulerFactory.CreateQuartzScheduler();
                    await _scheduler.Start();
                }),

                Task.Run(async () => await EnsureDatabaseAsync()),

                Task.Run(() => InitializeServiceHost())
            };

            Task.WaitAll(startupTasks.ToArray());
            ServiceHost.Open();
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
        }

        private async Task EnsureDatabaseAsync()
        {
            using (var context = new JobContext())
            {
                _logger.Info($"Migrating Database at {context.Database.GetDbConnection().DataSource}");
                await context.Database.MigrateAsync();
            }
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
