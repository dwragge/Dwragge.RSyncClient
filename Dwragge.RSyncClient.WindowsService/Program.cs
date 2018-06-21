using System;
using Topshelf;

namespace Dwragge.RSyncClient.WindowsService
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
                x.Service<ServiceWindowsService>();

                x.StartAutomatically();
                x.RunAsLocalService();
                x.EnableShutdown();

                x.SetServiceName("Rsync Management Client");
                x.SetDescription("Manages rsync backups to Azure Blob Storage");

                x.UseNLog();
            });

            var exitCode = host.Run();
            Environment.ExitCode = (int)exitCode;
        }
    }
}
