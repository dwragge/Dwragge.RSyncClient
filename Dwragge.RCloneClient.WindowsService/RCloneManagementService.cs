using System.Threading.Tasks;
using Dwragge.RCloneClient.Common;
using Quartz;

namespace Dwragge.RCloneClient.WindowsService
{
    public class RCloneManagementService : IRCloneManagementService
    {
        private readonly IScheduler _scheduler;

        public RCloneManagementService(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public async Task<string> HelloWorld()
        {
            var service = new RCloneService();
            var command = RCloneCommandBuilder.CreateCommand(RCloneSubCommand.Copy)
                .WithLocalPath("M:\\EU Photos\\")
                .WithRemote("azure")
                .WithRemotePath("backup/EU Photos")
                .AsDryRun()
                .WithDebugLogging()
                .Build();

            await service.ExecuteCommand(command);

            return command;
        }

        public async Task CreateTask(BackupFolderInfo info)
        {

        }

        public void PostHelloJob(string name)
        {
            
        }
    }
}
