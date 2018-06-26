using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.Persistence;
using Quartz;

namespace Dwragge.RCloneClient.WindowsService.Jobs
{
    public class RCloneJob : IJob
    {
        public string Command { get; set; }
        public async Task Execute(IJobExecutionContext context)
        {
            var service = new RCloneService();
            await service.ExecuteCommand(Command);
        }
    }
}
