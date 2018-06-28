using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.Persistence;
using Microsoft.EntityFrameworkCore;
using NLog;
using Quartz;

namespace Dwragge.RCloneClient.WindowsService.Jobs
{
    public class PreCheckMoveFilesJob : IJob
    {
        public RCloneCommand Command;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IJobContextFactory _contextFactory;
        private readonly IBackedUpFileTracker _tracker;

        public PreCheckMoveFilesJob(IJobContextFactory contextFactory, IBackedUpFileTracker tracker)
        {
            _contextFactory = contextFactory;
            _tracker = tracker;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Command = (RCloneCommand)context.MergedJobDataMap["Command"] ?? throw new ArgumentNullException(nameof(Command), "Command was not set");

            if (Command.SubCommand != RCloneSubCommand.Sync)
            {
                throw new InvalidOperationException("Must be sync command");
            }

            if (!Command.IsDryRun)
            {
                Command.IsDryRun = true;
            }

            try
            {
                using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var candidateFiles = await GetChangedOrNewFilesAsync();
                    var (changedFiles, newFiles) = await SortCandidateFilesAsync(candidateFiles);

                    var remotePath = $"{Command.RemoteName}:{Command.RemotePath}";
                    await _tracker.TrackNewFilesAsync(newFiles, Command.LocalPath, remotePath);
                    // mark files as not archived
                    //throw new NotImplementedException();
                    //transaction.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception: {ex.Message}");
            }
        }

        private async Task<HashSet<string>> GetChangedOrNewFilesAsync()
        {
            var files = new ConcurrentDictionary<string, byte>();
            bool invalidLineFound = false;

            void CaptureNoticeAction(string str)
            {
                if (str.Contains("NOTICE:") && str.Contains("Not copying as --dry-run"))
                {
                    var lineSplit = str.Split(':');
                    if (lineSplit.Length != 5)
                    {
                        invalidLineFound = true;
                        _logger.Error($"RClone Output contained an unexpected line syntax: {str}.");
                        return;
                    }

                    var fileName = lineSplit[3].Trim();
                    files.TryAdd(Path.Combine(Command.LocalPath, fileName), 0);
                    _logger.Trace($"Found file {fileName} that is either new or updated.");
                }
            }

            var service = new RCloneService();
            await service.ExecuteCommand(Command.CommandString, CaptureNoticeAction);
            if (invalidLineFound) throw new InvalidOperationException("Found an invalid line in RClone's output.");

            return new HashSet<string>(files.Keys);
        }

        private async Task<(IEnumerable<BackedUpFileDto> ChangedFiles, IEnumerable<string> NewFiles)> SortCandidateFilesAsync(HashSet<string> changedOrNewFiles)
        {
            using (var context = _contextFactory.CreateContext())
            {
                var trackedFiles = await context.BackedUpFiles.Where(f => changedOrNewFiles.Contains(f.FileName)).ToListAsync();
                var changedFiles = new List<BackedUpFileDto>();
                foreach (var trackedFile in trackedFiles)
                {
                    if (trackedFile.IsArchived && changedOrNewFiles.Contains(trackedFile.FileName))
                    {
                        changedFiles.Add(trackedFile);
                        changedOrNewFiles.Remove(trackedFile.FileName);
                    }
                }

                return (changedFiles, changedOrNewFiles);
            }
        }
    }
}
