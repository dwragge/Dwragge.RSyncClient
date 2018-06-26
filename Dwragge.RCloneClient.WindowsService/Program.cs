using System;
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
            Environment.ExitCode = (int)exitCode;
        }
    }
}
