using System;
using System.Linq;
using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using Dwragge.BlobBlaze.Web.Jobs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Dwragge.BlobBlaze.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateWebHostBuilder(args).Build();
            EnsureDatabase(builder.Services);

            var scheduler = builder.Services.GetService<IScheduler>();
            scheduler.JobFactory = builder.Services.GetService<DotnetCoreJobFactory>();
            scheduler.Start();

            LoadJobs(builder.Services);
            RestoreState(builder.Services);

            builder.Run();
        }

        private static void EnsureDatabase(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger<Program>>();
            using (var context = provider.GetService<IApplicationContextFactory>().CreateContext())
            {
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "Failed to migrate database: {message} \n {stacktrace}", e.Message, e.StackTrace);
                    throw;
                }
            }
        }


        private static void LoadJobs(IServiceProvider provider)
        {
            var logger = provider.GetService<ILogger<Program>>();
            logger.LogInformation("Beginning loading jobs from the database");
            using (var context = provider.GetService<IApplicationContextFactory>().CreateContext())
            {
                var folders = context.BackupFolders.ToList();
                var scheduler = provider.GetService<IScheduler>();

                foreach (var folder in folders)
                {
                    var (Job, Trigger) = QuartzJobFactory.CreateSyncJob(folder);
                    scheduler.ScheduleJob(Job, Trigger);
                    logger.LogInformation($"Creating sync job from database. Name = {folder.Name}, Path = {folder.Path}, Id = {folder.BackupFolderId}, Next Fire Time {Trigger.GetNextFireTimeUtc()?.ToLocalTime()}");

                    ScheduleSyncNowIfNecessary(folder, Job);
                }
            }
        }

        private static void ScheduleSyncNowIfNecessary(BackupFolder info, IJobDetail baseJob)
        {
            var triggerTimeToday = DateTime.Parse(info.SyncTime.ToString());
            if (triggerTimeToday < DateTime.Now)
            {
                // if we choose closest time, could never happen
                //var triggerTimeTomorrow = triggerTimeToday.AddDays(1);
                //var todayDiff = DateTime.Now - triggerTimeToday;
                //var tomorrowDiff = triggerTimeTomorrow - DateTime.Now;
                //var shouldTriggerNow = todayDiff < tomorrowDiff;
                //if (shouldTriggerNow)
                //{

                //}

                //_logger.Info($"Trigger for sync job {info.Name} is at {triggerTimeToday:t} which is in the past and closer to current time than tomorrow. Scheduling to run now.");
                //var triggerNow = TriggerBuilder.Create()
                //    .ForJob(baseJob)
                //    .StartNow()
                //    .Build();

                //_scheduler.ScheduleJob(triggerNow);
            }
        }

        private static void RestoreState(IServiceProvider provider)
        {
            // load in the backupfolderjobs that aren't yet complete
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
