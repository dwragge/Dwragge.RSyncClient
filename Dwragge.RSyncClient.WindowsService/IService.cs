using System.ServiceModel;

namespace Dwragge.RCloneClient.WindowsService
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        string HelloWorld();
    }
}
