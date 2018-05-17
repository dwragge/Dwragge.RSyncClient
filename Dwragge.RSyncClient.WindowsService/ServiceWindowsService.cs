using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceProcess;

namespace Dwragge.RSyncClient.WindowsService
{
    public class ServiceWindowsService : ServiceBase
    {
        public ServiceHost ServiceHost;

        public ServiceWindowsService()
        {
            ServiceName = "Dwragge.RSyncClient.Service";
        }
        protected override void OnStart(string[] args)
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
        }

        protected override void OnStop()
        {
            ServiceHost?.Close();
            ServiceHost = null;
        }
    }
}
