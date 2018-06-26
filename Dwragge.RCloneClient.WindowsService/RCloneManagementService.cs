using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
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
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public RCloneManagementService(IScheduler scheduler, IMapper mapper)
        {
            _scheduler = scheduler;
            _mapper = mapper;
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
                using (var context = new JobContext())
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

                ScheduleCopyJobToRunNow(info);
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

        public async Task<IEnumerable<BackupFolderDto>> GetBackupFolders()
        {
            using (var context = new JobContext())
            {
                return context.BackupFolders.ToList();
            }
        }

        private void ScheduleCopyJobToRunNow(BackupFolderInfo info)
        {
            var copyJob = JobBuilder.Create<RCloneJob>()
                .WithIdentity(info.Id.ToString(), "copy")
                .UsingJobData("Command", info.CopyCommand)
                .Build();
            var copyTrigger = TriggerBuilder.Create()
                .ForJob(copyJob)
                .StartNow()
                .Build();

            _scheduler.ScheduleJob(copyJob, copyTrigger);
        }

        public void PostHelloJob(string name)
        {
            
        }
    }
}
