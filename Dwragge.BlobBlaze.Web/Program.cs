using System;
using System.Linq;
using Dwragge.BlobBlaze.Application;
using Dwragge.BlobBlaze.Application.Requests;
using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using MediatR;
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

            builder.Services.GetService<IUploadProcessor>().Start();

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
            using (var scope = provider.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
                logger.LogInformation("Beginning loading jobs from the database");
                using (var context = scope.ServiceProvider.GetService<IApplicationContextFactory>().CreateContext())
                {
                    var folders = context.BackupFolders.AsNoTracking().Include(x => x.Remote).ToList();
                    var mediatr = scope.ServiceProvider.GetService<IMediator>();

                    foreach (var folder in folders)
                    {
                        mediatr.Send(new ScheduleJobForFolderRequest(folder)).Wait();
                        //ScheduleSyncNowIfNecessary(folder, job);
                    }
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
            using (var stateRestorer = provider.GetService<IStateRestorer>())
            {
                stateRestorer.RestoreState();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
