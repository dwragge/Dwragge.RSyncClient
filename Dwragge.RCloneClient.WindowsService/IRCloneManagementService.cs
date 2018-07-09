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
    }
}
