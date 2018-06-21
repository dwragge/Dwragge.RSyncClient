using System.ServiceModel;

namespace Dwragge.RCloneClient.WindowsService
{
    [ServiceContract]
    public interface IRCloneManagementService
    {
        [OperationContract]
        string HelloWorld();

        [OperationContract]
        void PostHelloJob(string name);
    }
}
