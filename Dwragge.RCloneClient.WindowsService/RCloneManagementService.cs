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
        public async Task<string> HelloWorld()
        {
            var builder = new StringBuilder();
            var keys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            foreach (var key in keys)
            {
                var job = await _scheduler.GetJobDetail(key);
                var trigger = await _scheduler.GetTriggersOfJob(key);
                builder.AppendLine(
                    $"Job {key.Name}:{key.Group} will next fire at {trigger.Single().GetNextFireTimeUtc()}");
            }

            return builder.ToString();
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

        public Task<IEnumerable<string>> GetRemotes()
        {
            var service = new RCloneService();
            return service.GetRemotes();
        }
        

        public void PostHelloJob(string name)
        {
            
        }
    }
}
