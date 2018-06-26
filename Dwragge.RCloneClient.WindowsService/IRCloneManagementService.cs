using System.ServiceModel;
using System.Threading.Tasks;
using Dwragge.RCloneClient.Persistence;

namespace Dwragge.RCloneClient.WindowsService
{
    [ServiceContract]
    public interface IRCloneManagementService
    {
        [OperationContract]
        Task<string> HelloWorld();

        [OperationContract]
        Task CreateTask(BackupFolderDto info);

        [OperationContract]
        void PostHelloJob(string name);
    }
}
