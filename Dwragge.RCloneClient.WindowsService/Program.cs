using System;
using Dwragge.RCloneClient.Common;
using NLog;
using Topshelf;

namespace Dwragge.RCloneClient.WindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = (Exception) args.ExceptionObject;
                LogManager.GetCurrentClassLogger().Fatal($"Unhandled exception: {ex.GetType().Name}. {ex.Message}");
            };

            var host = HostFactory.New(x =>
            {
                x.Service<WindowsServiceHost>();

                x.StartAutomatically();
                x.RunAsLocalService();
                x.EnableShutdown();

                x.SetServiceName("rclone Management Client");
                x.SetDescription("Manages rclone backups to Azure Blob Storage");

                x.UseNLog();
            });

            var exitCode = host.Run();

            if (DebugChecker.IsDebug)
            {
                Console.ReadKey();
            }

            Environment.ExitCode = (int)exitCode;
        }
    }
}
