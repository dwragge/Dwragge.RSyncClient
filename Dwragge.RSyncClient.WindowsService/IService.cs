using System.ServiceModel;

namespace Dwragge.RSyncClient.WindowsService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string HelloWorld();
    }
}
