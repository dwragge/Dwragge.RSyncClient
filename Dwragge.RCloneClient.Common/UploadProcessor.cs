using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ByteSizeLib;
using Dwragge.RCloneClient.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage;
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
                try
                {
                    _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    if (!await TryDequeueAndProcessFileAsync())
                    {
                        await Task.Delay(50).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    _logger.Fatal($"Exception Occurred In Upload Processor Thread: {e.GetType().Name}. {e.Message}");
                    throw;
                }
                //move to inprogress
                // wait for available thread
                // spawn thread
                // spin a few times then pause
                // pause/unpause needs to be syncronized well
            }
        }

        private async Task<bool> TryDequeueAndProcessFileAsync()
        {
            bool found = false;
            InProgressFileDto inProgress = null;
            using (var context = _factory.CreateContext(false))
            {
                var file = await context.PendingFiles.FirstOrDefaultAsync().ConfigureAwait(false);
                if (file != null)
                {
                    found = true;
                    context.PendingFiles.Remove(file);

                    _logger.Debug($"Popped {file.FileName} from queue to be transferred");
                    var remoteFileName =
                        Path.Combine(file.BackupFolder.RemoteBaseFolder,
                            file.FileName.Replace(file.BackupFolder.Path, "").Remove(0, 1), file.QueuedTime.ToString("yyyy-mm-dd")).Replace('\\', '/');

                    inProgress = new InProgressFileDto()
                    {
                        BackupFolderId = file.BackupFolderId,
                        FileName = file.FileName,
                        InsertedAt = DateTime.Now,
                        RemotePath = remoteFileName
                    };
                    context.InProgressFiles.Add(inProgress);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            if (found)
            {
                await _uploadTaskSemaphore.WaitAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
                _runningTasks.Add(Task.Run(() => UploadFile(inProgress))); 
            }

            return found;
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
            var g = Guid.NewGuid().ToString().Substring(0, 8);
            try
            {
                _logger.Info($"{{{g}}} Uploading {dto.FileName} on thread {Thread.CurrentThread.ManagedThreadId}");

                
                        
                var localFile = new FileInfo(dto.FileName);

                var account = CloudStorageAccount.DevelopmentStorageAccount;
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference("test");
                var blob = container.GetBlockBlobReference(dto.RemotePath);

                _logger.Debug($"{{{g}}} Using Cloud Storage Account {account.BlobStorageUri.PrimaryUri}");
                _logger.Debug($"{{{g}}} Uploading to {container.Name}/{dto.RemotePath}");
                _logger.Info(
                    $"{{{g}}} Beginning Upload of {dto.FileName}. File Size is {ByteSize.FromBytes(localFile.Length).ToString()}");

                var startTime = DateTime.Now;
                using (var fs = File.OpenRead(dto.FileName))
                {
                    await blob.UploadFromStreamAsync(fs);
                }

                var secondsTime = (DateTime.Now - startTime).TotalSeconds;
                _logger.Info(
                    $"{{{g}}} Finished uploading {dto.FileName} in {secondsTime:0.##}s. Average Speed was {ByteSize.FromBytes(localFile.Length / secondsTime).ToString()}/s.");
                // upload file
                // save version information if necessary
                // move to tracked files
                // set to archive

                using (var context = _factory.CreateContext(false))
                using (var trans = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted))
                {
                    var trackedFile = new TrackedFileDto
                    {
                        BackupFolderId = dto.BackupFolderId,
                        FileName = dto.FileName,
                        FirstBackedUp = DateTime.Now,
                        LastModified = localFile.LastWriteTimeUtc,
                        SizeBytes = localFile.Length,
                        RemoteLocation = dto.RemotePath
                    };

                    context.InProgressFiles.Remove(dto);

                    var existingFile = await context.TrackedFiles.SingleOrDefaultAsync(t => t.FileName == dto.FileName);
                    if (existingFile != null)
                    {
                        _logger.Info($"{{{g}}} Removing old version and adding it to version tracking.");

                        var versionHistory = new FileVersionHistoryDto()
                        {
                            BackupFolderId = dto.BackupFolderId,
                            FileName = dto.FileName,
                            RemoteLocation = existingFile.RemoteLocation,
                            VersionedOn = DateTime.Today
                        };

                        await context.FileVersionHistory.AddAsync(versionHistory);
                        context.TrackedFiles.Remove(existingFile);
                        await context.SaveChangesAsync();
                    }

                    context.TrackedFiles.Add(trackedFile);
                    await context.SaveChangesAsync(_cancellationTokenSource.Token);
                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                _logger.Error($"{{{g}}} Exception occurred while uploading file {e.GetType().Name}: {e.Message}");
            }
            finally
            {
                _uploadTaskSemaphore.Release();
            }
        }
    }
}
