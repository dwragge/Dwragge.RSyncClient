using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Dwragge.RCloneClient.WindowsService.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NLog;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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

                InitializeServiceHost();
                InitializeIoC();
                EnsureDatabase();
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

            var command = new RCloneCommand(RCloneSubCommand.Sync)
            {
                LocalPath = @"M:\EU Photos",
                RemoteName = "azure",
                RemotePath = "backup/EU-Photos",
                IsDryRun = true,
                WithDebugLogging = true
            };

            var job = JobBuilder.Create<PreCheckMoveFilesJob>()
                .WithIdentity("test")
                .Build();
            job.JobDataMap["Command"] = command;
            var trigger = TriggerBuilder.Create()
                .ForJob(job)
                .StartNow()
                .Build();
            _scheduler.ScheduleJob(job, trigger);

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
        }

        private void InitializeIoC()
        {
            _container = BuildContainer();
            ServiceHost.AddDependencyInjectionBehavior<IRCloneManagementService>(_container);
            var jobFactory = new AutofacJobFactory(_container);
            _scheduler.JobFactory = jobFactory;
        }

        private void LoadJobs()
        {
            using (var scope = _container.BeginLifetimeScope())
            {
                IList<BackupFolderDto> syncedFolders;
                using (var context = scope.Resolve<IJobContextFactory>().CreateContext())
                {
                    syncedFolders = context.BackupFolders.ToList();
                }

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
            using (var context = _container.Resolve<IJobContextFactory>().CreateContext())
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
            builder.RegisterType<JobContextFactory>().As<IJobContextFactory>();
            builder.RegisterType<BackedUpFileTracker>().As<IBackedUpFileTracker>();
            builder.RegisterInstance(new LoggerFactory(new List<ILoggerProvider>
            {
                new NLogLoggerProvider()
            })).As<ILoggerFactory>();

            foreach (var jobType in Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(IJob))))
            {
                builder.RegisterType(jobType);
            }

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
