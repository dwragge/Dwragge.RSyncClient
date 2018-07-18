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
        Task<bool> Heartbeat();

        [OperationContract]
        Task CreateTask(BackupFolderDto info);

        [OperationContract]
        Task AddOrUpdateRemote(RemoteDto remote);

        [OperationContract]
        RemoteDto[] GetRemotes();

        [OperationContract]
        Task DeleteRemote(RemoteDto dto);

        [OperationContract]
        Task GetBackupFolders(int remoteId);
    }
}
