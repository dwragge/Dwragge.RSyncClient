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
    // TODO : Split into Upload and UploadProcessor
    // Currently violates SRP pretty badly 
    // Two jobs of this currently are 1) reading off the queue and managing threads and 2) uploading the file to Azure
    // Maybe split into Upload and have AzureUpload S3Upload w/e eventually
    public class UploadProcessor : IUploadProcessor
    {
        private readonly IApplicationContextFactory _factory;
        private readonly ILogger _logger;

        private bool _isRunning;
        private readonly SemaphoreSlim _uploadTaskSemaphore = new SemaphoreSlim(MaxUploadThreads, MaxUploadThreads);
        private readonly BlockingCollection<BackupFileUploadJob> _jobQueue = new BlockingCollection<BackupFileUploadJob>();
        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundTask;
        private readonly List<Task> _runningTasks = new List<Task>();
        private readonly List<BackupFileUploadJob> _failedJobs = new List<BackupFileUploadJob>();
        private const int MaxUploadThreads = 4;
        private const int MaxNumRetries = 0;

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
            }
        }

        private async Task<bool> TryDequeueAndProcessFileAsync()
        {
            var took = _jobQueue.TryTake(out var job, 500);
            if (!took) return false;
            if (job.ParentJob.Status != BackupFolderJobStatus.InProgress)
            {
                using (var context = _factory.CreateContext())
                {
                    context.BackupJobs.Attach(job.ParentJob);
                    job.ParentJob.Status = BackupFolderJobStatus.InProgress;
                    await context.SaveChangesAsync();
                }
            }


            await _uploadTaskSemaphore.WaitAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            _runningTasks.Add(Task.Run(() => UploadFile(job).ContinueWith(task => OnUploadComplete(job))));

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
            if (job.ParentJob.Folder.Remote.ConnectionString.IsDevelopment)
            {
                account = CloudStorageAccount.DevelopmentStorageAccount;
            }
            else
            {
                if (!CloudStorageAccount.TryParse(job.ParentJob.Folder.Remote.ConnectionString.ToString(), out account))
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

        private async Task OnUploadComplete(BackupFileUploadJob job)
        {
            using (var context = _factory.CreateContext())
            {
                context.BackupJobs.Attach(job.ParentJob);
                if (job.ParentJob.Status != BackupFolderJobStatus.InProgress)
                {
                    context.Entry(job.ParentJob).State = EntityState.Modified;
                    _logger.LogInformation($"Finished Processing all files for folder {job.ParentJob.Folder.Name} at path {job.ParentJob.Folder.Path}");
                }

                await context.SaveChangesAsync();
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

                await TrackFile(job);
                job.ParentJob.IncrementComplete();
                // set to archive
            }
            catch (Exception e)
            {
                _logger.LogError($"{{{g}}} Exception occurred while uploading file {e.GetType().Name}: {e.Message}");
                await LogError(job, e);
                if (job.RetryCount < MaxNumRetries)
                {
                    _logger.LogInformation($"{{{g}}} Retrying job... {job.RetryCount} of {MaxNumRetries}");
                    _jobQueue.Add(job);
                    job.RetryCount++;
                }
                else
                {
                    job.ParentJob.IncrementErrored();
                }
            }
            finally
            {
                _uploadTaskSemaphore.Release();
            }
        }

        private async Task LogError(BackupFileUploadJob job, Exception e)
        {
            try
            {
                using (var context = _factory.CreateContext())
                {
                    var uploadError = new UploadError(job, e);
                    await context.UploadErrors.AddAsync(uploadError);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // this can't throw
            }
        }
    }
}

