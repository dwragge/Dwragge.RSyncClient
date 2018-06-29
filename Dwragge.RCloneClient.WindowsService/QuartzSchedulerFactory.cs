using System;
using System.Collections.Specialized;
using System.IO;
using Dwragge.RCloneClient.Common;
using NLog;
using Quartz;
using Quartz.Impl;

namespace Dwragge.RCloneClient.WindowsService
{
    internal class QuartzSchedulerFactory
    {
        private static Logger Logger => LogManager.GetCurrentClassLogger();

        public static IScheduler CreateQuartzScheduler()
        {
            //return DebugChecker.IsDebug ? CreateDefaultScheduler() : CreateSchedulerWithSqliteStore();
            return CreateDefaultScheduler();
        }

        private static IScheduler CreateDefaultScheduler()
        {
            var props = new NameValueCollection
            {
                {"quartz.serializer.type", "binary"}
            };

            var schedulerFactory = new StdSchedulerFactory(props);
            var scheduler = schedulerFactory.GetScheduler().Result;

            return scheduler;
        }

        private static IScheduler CreateSchedulerWithSqliteStore()
        {
            var dbPath = GetDbLocation();
            TryCreateDb(dbPath);

            var props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" },
                { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" },
                { "quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz" },
                { "quartz.jobStore.tablePrefix", "QRTZ_" },
                { "quartz.jobStore.dataSource", "sqlite" },
                { "quartz.dataSource.sqlite.connectionString",  $"Data Source={dbPath}"},
                { "quartz.dataSource.sqlite.provider", "SQLite" }
            };

            var schedulerFactory = new StdSchedulerFactory(props);
            var scheduler = schedulerFactory.GetScheduler().Result;

            return scheduler;
        }

        private static void TryCreateDb(string dbPath)
        {
            if (string.IsNullOrEmpty(dbPath)) throw new ArgumentNullException(nameof(dbPath));

            if (File.Exists(dbPath))
            {
                Logger.Info($"Found DB Path as {dbPath}");
                return;
            }

            Logger.Info($"Creating SQLite Database at {dbPath}");
            CreateDbFile(dbPath);

            Logger.Info("Initializing Tables...");
        }

        private static void CreateDbFile(string dbPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? throw new IOException("Failed to create Directory, Directory.CreateDirectory returned null"));
                File.Open(dbPath, FileMode.OpenOrCreate).Close();
            }
            catch (IOException ex)
            {
                Logger.Error(ex, $"Failed to create the path {dbPath}");
                throw;
            }
        }
        

        private static string GetDbLocation()
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(baseFolder, "rcloneservice", "jobstore.db");
        }
    }
}
