using ByteSizeLib;
using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dwragge.BlobBlaze.Application
{
    public class UploadProcessor : IUploadProcessor
    {
        private readonly IApplicationContextFactory _factory;

        private bool _isRunning;
        private readonly SemaphoreSlim _uploadTaskSemaphore = new SemaphoreSlim(MaxUploadThreads, MaxUploadThreads);
        private BlockingCollection<BackupFileUploadJob> _jobQueue = new BlockingCollection<BackupFileUploadJob>();
        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundTask;
        private readonly List<Task> _runningTasks = new List<Task>();
        private readonly List<BackupFileUploadJob> _failedJobs = new List<BackupFileUploadJob>();
        private const int MaxUploadThreads = 4;

        private readonly ILogger _logger;

        public IReadOnlyCollection<BackupFileUploadJob> FailedJobs => _failedJobs.AsReadOnly();

        public UploadProcessor(IApplicationContextFactory factory, ILogger<UploadProcessor> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public void Start()
        {
            if (_isRunning) return;

            _logger.LogInformation("Starting Upload Processor thread");
            _isRunning = true;
            _backgroundTask = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public void AddJob(BackupFileUploadJob job)
        {
            _jobQueue.Add(job);
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
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, $"Exception Occurred In Upload Processor Thread: {e.GetType().Name}. {e.Message}. \n{e.StackTrace}");
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
            var took = _jobQueue.TryTake(out var job, 500);
            if (!took) return false;

            job.Status = BackupFileUploadJobStatus.InProgress;
            await _uploadTaskSemaphore.WaitAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            _runningTasks.Add(Task.Run(() => UploadFile(job)));

            return true;
        }

        public void NotifyOfPendingTasks()
        {
            // unpause
        }

        public async Task Shutdown()
        {
            try
            {
                _jobQueue.Dispose();
                _cancellationTokenSource.Cancel();
                await _backgroundTask;
                await Task.WhenAll(_runningTasks);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private CloudStorageAccount GetStorageAccount(BackupFileUploadJob job)
        {
            CloudStorageAccount account;
            if (job.ParentJob.Folder.Remote.ConnectionString == "deveopment")
            {
                account = CloudStorageAccount.DevelopmentStorageAccount;
            }
            else
            {
                if (!CloudStorageAccount.TryParse(job.ParentJob.Folder.Remote.ConnectionString, out account))
                {
                    throw new InvalidOperationException("Connection String was Invalid");
                }
            }

            return account;
        }

        private async Task TrackFile(BackupFileUploadJob job)
        {
            using (var context = _factory.CreateContext())
            {
                var alreadyTracked = await context.TrackedFiles.Include(f => f.BackupFolder)
                    .SingleOrDefaultAsync(f => f.FileName == job.LocalFile.FullName);
                if (alreadyTracked != null)
                {
                    var version = new TrackedFileVersion(alreadyTracked);
                    await context.TrackedFileVersions.AddAsync(version, _cancellationTokenSource.Token);
                    alreadyTracked.UpdateFromFileInfo(job.LocalFile);
                }
                else // new file
                {
                    var trackedFile = new TrackedFile(job);
                    await context.TrackedFiles.AddAsync(trackedFile, _cancellationTokenSource.Token);
                }

                await context.SaveChangesAsync(_cancellationTokenSource.Token);
            }
        }

        private async Task UploadFile(BackupFileUploadJob job)
        {
            var g = Guid.NewGuid().ToString().Substring(0, 8);
            try
            {
                _logger.LogDebug($"{{{g}}} Uploading {job.UploadPath} on thread {Thread.CurrentThread.ManagedThreadId}");

                var account = GetStorageAccount(job);
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference("test");
                var blob = container.GetBlockBlobReference(job.UploadPath);

                _logger.LogDebug($"{{{g}}} Using Cloud Storage Account {account.BlobStorageUri.PrimaryUri}");
                _logger.LogDebug($"{{{g}}} Uploading to {container.Name}/{job.UploadPath}");
                _logger.LogInformation(
                    $"{{{g}}} Beginning Upload of {job.UploadPath}. File Size is {ByteSize.FromBytes(job.LocalFile.Length).ToString()}");

                var startTime = DateTime.UtcNow;
                using (var fs = File.OpenRead(job.LocalFile.FullName))
                {
                    await blob.UploadFromStreamAsync(fs, AccessCondition.GenerateEmptyCondition(), null, null, _cancellationTokenSource.Token);
                }
                job.Status = BackupFileUploadJobStatus.Successful;

                var secondsTime = (DateTime.UtcNow - startTime).TotalSeconds;
                _logger.LogInformation(
                    $"{{{g}}} Finished uploading {job.UploadPath} in {secondsTime:0.##}s. Average Speed was {ByteSize.FromBytes(job.LocalFile.Length / secondsTime).ToString()}/s.");

                // set to archive

                await TrackFile(job);
            }
            catch (Exception e)
            {
                _logger.LogError($"{{{g}}} Exception occurred while uploading file {e.GetType().Name}: {e.Message}");
                if (job.RetryCount < 3)
                {
                    job.Status = BackupFileUploadJobStatus.ErroredRetrying;
                    _jobQueue.Add(job);
                }
                else
                {
                    job.Status = BackupFileUploadJobStatus.Errored;
                }
            }
            finally
            {
                _uploadTaskSemaphore.Release();
            }
        }
    }
}

