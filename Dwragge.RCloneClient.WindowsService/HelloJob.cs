using System.Threading.Tasks;
using NLog;
using Quartz;

namespace Dwragge.RSyncClient.WindowsService
{
    public class HelloJob : IJob
    {
        public string Name { get; set; }
        public Task Execute(IJobExecutionContext context)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"Hello {Name}");

            return Task.FromResult(true);
        }
    }
}