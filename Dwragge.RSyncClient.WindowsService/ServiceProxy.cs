using System.ServiceModel;
using System.ServiceModel.Description;

namespace Dwragge.RSyncClient.WindowsService
{
    public class ServiceProxy : ClientBase<IService>
    {
        public ServiceProxy()
            : base(new ServiceEndpoint(ContractDescription.GetContract(typeof(IService)),
                new NetNamedPipeBinding(),
                new EndpointAddress("net.pipe://localhost/com.Dwragge.RsyncClientService/hello")))
        {

        }

        public string InvokeHelloWorld()
        {
            return Channel.HelloWorld();
        }
    }
}
