using System.Threading.Tasks;

namespace Dwragge.RCloneClient.Common
{
    public interface IUploadProcessor
    {
        void NotifyOfPendingTasks();
        Task Shutdown();
        void Start();
    }
}