using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.Persistence;
using NLog;
using Quartz;

namespace Dwragge.RCloneClient.WindowsService.Jobs
{
    public class RCloneJob : IJob
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public Guid Id { get; } = Guid.NewGuid();
        public string Command { get; set; }
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.Info($"Executing RCloneJob {Id}, fired at {context.FireTimeUtc.ToLocalTime()}.");
            var startTime = DateTime.Now;
            var service = new RCloneService();
            try
            {
                await service.ExecuteCommand(Command);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to execute task {Id} with error {ex.Message}");
            }
            _logger.Info($"RCloneJob {Id} Finished in {DateTime.Now - startTime:g}. Next Fire Time is at {context.NextFireTimeUtc?.ToLocalTime().ToString() ?? "N/A"}");
        }
    }
}

