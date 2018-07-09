using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.Persistence;
using Dwragge.RCloneClient.WindowsService.Jobs;
using NLog;
using Quartz;
using Quartz.Impl.Matchers;

namespace Dwragge.RCloneClient.WindowsService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class RCloneManagementService : IRCloneManagementService
    {
        private readonly IScheduler _scheduler;
        private readonly IMapper _mapper;
        private readonly IJobContextFactory _contextFactory;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public RCloneManagementService(IScheduler scheduler, IMapper mapper, IJobContextFactory contextFactory)
        {
            _scheduler = scheduler;
            _mapper = mapper;
            _contextFactory = contextFactory;
        }

        public Task<bool> Heartbeat()
        {
            return Task.FromResult(true);
        }

        public async Task CreateTask(BackupFolderDto dto)
        {
            try
            {
                using (var context = _contextFactory.CreateContext())
                {
                    var exists = context.BackupFolders.SingleOrDefault(x => x.Path == dto.Path) != null;
                    if (exists)
                    {
                        throw new InvalidOperationException($"Folder {dto.Path} already exists!");
                    }

                    await context.BackupFolders.AddAsync(dto);
                    await context.SaveChangesAsync();
                }

                var info = _mapper.Map<BackupFolderDto, BackupFolderInfo>(dto);
                var syncJob = QuartzJobFactory.CreateSyncJob(info);
                await _scheduler.ScheduleJob(syncJob.Job, syncJob.Trigger);

            }
            catch (Exception e)
            {
                _logger.Error($"Failed to Create Task: {e.Message}");
                throw;
            }
        }
    }
}
