using System.Threading.Tasks;
using NLog;
using Quartz;

namespace Dwragge.RSyncClient.WindowsService
{
    public class HelloJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Hello from Quartz");

            return Task.FromResult(true);
        }
    }
}