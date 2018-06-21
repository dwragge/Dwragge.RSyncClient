using System.Threading.Tasks;
using Quartz;

namespace Dwragge.RCloneClient.WindowsService.Jobs
{
    public class CopyJob : IJob
    {
        public string Path { get; set; }

        public Task Execute(IJobExecutionContext context)
        {
            // Path must be a folder
            // Path must exist
            throw new System.NotImplementedException();
        }
    }
}