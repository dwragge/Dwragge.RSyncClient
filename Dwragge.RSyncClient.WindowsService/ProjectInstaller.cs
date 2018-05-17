using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Dwragge.RSyncClient.WindowsService
{
    [RunInstaller(true)]
    public class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            var process = new ServiceProcessInstaller {Account = ServiceAccount.LocalSystem};
            var service = new ServiceInstaller {ServiceName = "Dwragge.RSyncClient.Service"};
            Installers.Add(process);
            Installers.Add(service);
        }
    }
}
