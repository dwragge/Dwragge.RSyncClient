using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.Persistence;
using NLog;
using Quartz;

namespace Dwragge.RCloneClient.WindowsService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class RCloneManagementService : IRCloneManagementService
    {
        private readonly IScheduler _scheduler;
        private readonly IJobContextFactory _contextFactory;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public RCloneManagementService(IScheduler scheduler, IJobContextFactory contextFactory)
        {
            _scheduler = scheduler;
            _contextFactory = contextFactory;
        }

        public Task<bool> Heartbeat()
        {
            _logger.Info("Sent heartbeat");
            return Task.FromResult(true);
        }

        public async Task CreateTask(BackupFolderDto dto)
        {
            _logger.Info($"Beginning creating backup folder for {dto.Path}...");

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
                
                var syncJob = QuartzJobFactory.CreateSyncJob(dto);
                await _scheduler.ScheduleJob(syncJob.Job, syncJob.Trigger);
                _logger.Info($"Successfully created backup folder, scheduled to run next at {syncJob.Trigger.GetNextFireTimeUtc()}");
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to Create backup folder: {e.Message}");
                throw;
            }
        }

        public async Task AddOrUpdateRemote(RemoteDto remote)
        {
            _logger.Info($"Creating or Updating remote {remote.Name}");
            var stringBytes = Encoding.UTF8.GetBytes(remote.ConnectionString);
            var protectedString = ProtectedData.Protect(stringBytes, null, DataProtectionScope.CurrentUser);
            var protectedBase64 = Convert.ToBase64String(protectedString);
            remote.ConnectionString = protectedBase64;

            using (var context = _contextFactory.CreateContext())
            {
                var exists = context.Remotes.Find(remote.RemoteId);
                if (exists != null)
                {
                    exists.Name = remote.Name;
                    exists.ConnectionString = remote.ConnectionString;
                }
                else
                {
                    context.Add(remote);
                }

                await context.SaveChangesAsync();
            }
            _logger.Info($"Successfully created remote {remote.Name}");
        }

        public RemoteDto[] GetRemotes()
        {
            _logger.Info("Beginning getting remotes...");
            IEnumerable<RemoteDto> remotes;
            using (var context = _contextFactory.CreateContext())
            {
                remotes = context.Remotes.ToList();
                
            }

            var decrypted = remotes.Select(r =>
            {
                _logger.Debug($"Decrypting remote {r.Name}");

                var protectedBytes = Convert.FromBase64String(r.ConnectionString);
                var stringBytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                r.ConnectionString = Encoding.UTF8.GetString(stringBytes);
                return r;
            }).ToArray();

            _logger.Info($"Returned {decrypted.Length} remotes.");
            return decrypted;
        }

        public async Task DeleteRemote(RemoteDto dto)
        {
            _logger.Info($"Deleting remote {dto.Name}");
            using (var context = _contextFactory.CreateContext())
            {
                context.Remotes.Remove(dto);
                await context.SaveChangesAsync();
            }
        }
    }
}
