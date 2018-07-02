using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dwragge.RCloneClient.Persistence;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Dwragge.RCloneClient.Common
{
    public class UploadProcessor : IUploadProcessor
    {
        private readonly IJobContextFactory _factory;

        private bool _isRunning;
        private readonly SemaphoreSlim _uploadTaskSemaphore;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundTask;
        private readonly List<Task> _runningTasks;
        private const int MaxUploadThreads = 4;

        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UploadProcessor(IJobContextFactory factory)
        {
            _factory = factory;
            _runningTasks = new List<Task>();
            _uploadTaskSemaphore = new SemaphoreSlim(MaxUploadThreads, MaxUploadThreads);
        }

        public void Start()
        {
            _logger.Info($"Starting Upload Processor thread");
            _isRunning = true;
            _backgroundTask = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        private async Task Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            while (_isRunning)
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                //dequeue
                bool found = false;
                using (var context = _factory.CreateContext(false))
                {
                    var file = await context.PendingFiles.FirstOrDefaultAsync().ConfigureAwait(false);
                    if (file != null)
                    {
                        found = true;
                        context.PendingFiles.Remove(file);

                        _logger.Debug($"Popped {file.FileName} from queue to be transferred");
                        var inprogress = new InProgressFileDto()
                        {
                            BackupFolderId = file.BackupFolderId,
                            FileName = file.FileName,
                            InsertedAt = DateTime.Now
                        };
                        context.InProgressFiles.Add(inprogress);
                        await context.SaveChangesAsync().ConfigureAwait(false);

                        await _uploadTaskSemaphore.WaitAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
                        _runningTasks.Add(Task.Run(() => UploadFile(inprogress)));
                    }
                }

                if (!found)
                {
                    await Task.Delay(50);
                }
                //move to inprogress
                // wait for available thread
                // spawn thread
                // spin a few times then pause
                // pause/unpause needs to be syncronized well
            }
        }

        public void NotifyOfPendingTasks()
        {
            // unpause
        }

        public async Task Shutdown()
        {
            try
            {
                _cancellationTokenSource.Cancel();   
                await _backgroundTask;
                await Task.WhenAll(_runningTasks);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task UploadFile(InProgressFileDto dto)
        {
            try
            {
                _logger.Info($"Uploading {dto.FileName} on thread {Thread.CurrentThread.ManagedThreadId}");
                await Task.Delay(2000, _cancellationTokenSource.Token);
                _logger.Info($"Finished uploading {dto.FileName} on thread {Thread.CurrentThread.ManagedThreadId}");
                // upload file
                // save version information if necessary
                // move to tracked files
                // set to archive

                using (var context = _factory.CreateContext(false))
                {
                    var file = new FileInfo(dto.FileName);
                    var trackedFile = new TrackedFileDto()
                    {
                        BackupFolderId = dto.BackupFolderId,
                        FileName = dto.FileName,
                        FirstBackedUp = DateTime.Now,
                        LastModified = file.LastWriteTimeUtc,
                        SizeBytes = file.Length
                    };

                    context.InProgressFiles.Remove(dto);
                    context.TrackedFiles.Add(trackedFile);

                    var existingFile = await context.TrackedFiles.SingleOrDefaultAsync(t => t.FileName == dto.FileName);
                    if (existingFile != null)
                    {
                        _logger.Info($"Removing old version and adding it to version tracking.");
                        _logger.Info($"Not implemented yet tho.");
                        throw new NotImplementedException();
                    }

                    await context.SaveChangesAsync(_cancellationTokenSource.Token);
                }
            }
            finally
            {
                _uploadTaskSemaphore.Release();
            }
        }
    }
}
