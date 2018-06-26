using System.Collections.Generic;
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
        Task<IEnumerable<string>> GetRemotes();

        [OperationContract]
        Task<IEnumerable<BackupFolderDto>> GetBackupFolders();

        [OperationContract]
        void PostHelloJob(string name);
    }
}
