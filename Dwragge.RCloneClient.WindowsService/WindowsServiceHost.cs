using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using Autofac;
using Dwragge.RCloneClient.Common;
using Quartz;
using Topshelf;
using Autofac.Integration.Wcf;
using AutoMapper;
using Dwragge.RCloneClient.Common.AutoMapper;
using Dwragge.RCloneClient.Persistence;
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
                    var syncJob = QuartzJobFactory.CreateSyncJob(info);

                    _scheduler.ScheduleJob(syncJob.Job, syncJob.Trigger);
                    _logger.Info($"Creating sync job from database. Name = {info.Name}, Path = {info.Path}, Id = {info.Id}, Next Fire Time {syncJob.Trigger.GetNextFireTimeUtc()?.ToLocalTime()}");

                    ScheduleSyncNowIfNecessary(info, syncJob.Job);
                }
            }
        }

        private void ScheduleSyncNowIfNecessary(BackupFolderInfo info, IJobDetail baseJob)
        {
            var triggerTimeToday = DateTime.Parse($"{info.SyncTime.Hour}:{info.SyncTime.Minute}");
            if (triggerTimeToday < DateTime.Now)
            {
                var triggerTimeTomorrow = triggerTimeToday.AddDays(1);
                var todayDiff = DateTime.Now - triggerTimeToday;
                var tomorrowDiff = triggerTimeTomorrow - DateTime.Now;
                var shouldTriggerNow = todayDiff < tomorrowDiff;
                if (shouldTriggerNow)
                {
                    _logger.Info($"Trigger for sync job {info.Name} is at {triggerTimeToday:t} which is in the past and closer to current time than tomorrow. Scheduling to run now.");
                    var triggerNow = TriggerBuilder.Create()
                        .ForJob(baseJob)
                        .StartNow()
                        .Build();

                    _scheduler.ScheduleJob(triggerNow);
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
            builder.RegisterAutoMapper();

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
