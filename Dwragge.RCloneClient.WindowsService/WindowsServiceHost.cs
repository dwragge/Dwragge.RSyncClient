using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac;
using Dwragge.RCloneClient.Common;
using Quartz;
using Topshelf;
using Autofac.Integration.Wcf;
using AutoMapper;
using Dwragge.RCloneClient.Persistence;
using Dwragge.RCloneClient.Persistence.AutoMapperResolvers;
using Dwragge.RCloneClient.WindowsService.Jobs;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Dwragge.RCloneClient.WindowsService
{
    public class WindowsServiceHost : ServiceControl
    {
        public ServiceHost ServiceHost;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IScheduler _scheduler;
        private IContainer _container;
        
        public bool Start(HostControl hostControl)
        {
            try
            {
                _scheduler = QuartzSchedulerFactory.CreateQuartzScheduler();
                _scheduler.Start();
                EnsureDatabase();
                InitializeServiceHost();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex.InnerException, $"Failed to initialize: {ex.Message}");
                return false;
            }

            using (var scope = _container.BeginLifetimeScope())
            {
                var mapper = scope.Resolve<IMapper>();
                try
                {
                    mapper.ConfigurationProvider.AssertConfigurationIsValid();
                }
                catch (AutoMapperConfigurationException ex)
                {
                    var errors = ex.Errors.Select(x =>
                        $"{x.TypeMap.SourceType.Name} -> {x.TypeMap.DestinationType.Name}, CanConstruct: {x.CanConstruct}, UnmappedProperties: {string.Join(", ", x.UnmappedPropertyNames)}");

                    _logger.Fatal($"Failed to register AutoMapper, the following errors were found: {string.Join("\n", errors)}");
                    return false;
                }
            }

            try
            {
                LoadJobs();
            }
            catch (Exception e)
            {
                _logger.Fatal(e, $"Failed to load jobs: {e.Message}");
                return false;
            }

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

            _container = BuildContainer();
            ServiceHost.AddDependencyInjectionBehavior<IRCloneManagementService>(_container);
        }

        private void LoadJobs()
        {
            IList<BackupFolderDto> syncedFolders;
            using (var context = new JobContext())
            {
                syncedFolders = context.BackupFolders.ToList();
            }

            using (var scope = _container.BeginLifetimeScope())
            {
                var mapper = scope.Resolve<IMapper>();
                foreach (var folder in syncedFolders)
                {
                    var info = mapper.Map<BackupFolderInfo>(folder);

                    var job = JobBuilder.Create<RCloneJob>()
                        .WithIdentity(folder.Name, "sync")
                        .UsingJobData("Command", info.SyncCommand)
                        .Build();

                    var trigger = TriggerBuilder.Create()
                        .ForJob(job)
                        .WithCronSchedule($"0 {info.SyncTime.Minute} {info.SyncTime.Hour} 1/{info.SyncTimeSpan.Days} * ?")
                        .Build();

                    _scheduler.ScheduleJob(job, trigger);
                }
            }
        }

        private void EnsureDatabase()
        {
            using (var context = new JobContext())
            {
                _logger.Info($"Migrating Database at {context.Database.GetDbConnection().DataSource}");
                context.Database.Migrate();
            }
        }

        private IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_scheduler);
            builder.RegisterType<RCloneManagementService>().As<IRCloneManagementService>();
            builder.Register(c => new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<BackupFolderDto, BackupFolderInfo>()
                        .ForMember(dest => dest.SyncTime, opt => opt.ResolveUsing<BackupFolderSyncTimeResolver>());
                }))
                .AsImplementedInterfaces()
                .SingleInstance();
            builder.Register(c => c.Resolve<IConfigurationProvider>().CreateMapper()).As<IMapper>();

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
