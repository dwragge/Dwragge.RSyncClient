using System.ServiceModel;
using System.Threading.Tasks;

namespace Dwragge.RCloneClient.WindowsService
{
    [ServiceContract]
    public interface IRCloneManagementService
    {
        [OperationContract]
        Task<string> HelloWorld();

        [OperationContract]
        void PostHelloJob(string name);
    }
}
