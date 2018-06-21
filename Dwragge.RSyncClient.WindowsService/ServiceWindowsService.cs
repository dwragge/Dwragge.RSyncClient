using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;
using Topshelf;

namespace Dwragge.RCloneClient.WindowsService
{
    public class ServiceWindowsService : ServiceControl
    {
        public ServiceHost ServiceHost;

        public bool Start(HostControl hostControl)
        {
            ServiceHost?.Close();
            
            var baseAddress = "net.pipe://localhost/com.Dwragge.RsyncClientService";
            ServiceHost = new ServiceHost(typeof(Service), new Uri(baseAddress));
            ServiceHost.AddServiceEndpoint(typeof(IService), new NetNamedPipeBinding(), baseAddress);

            var behaviour = new ServiceMetadataBehavior();
            ServiceHost.Description.Behaviors.Add(behaviour);
            ServiceHost.AddServiceEndpoint(typeof(IMetadataExchange),
                MetadataExchangeBindings.CreateMexNamedPipeBinding(), baseAddress + "/mex/");

            ServiceHost.Open();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            ServiceHost?.Close();
            ServiceHost = null;
            return true;
        }
    }
}
