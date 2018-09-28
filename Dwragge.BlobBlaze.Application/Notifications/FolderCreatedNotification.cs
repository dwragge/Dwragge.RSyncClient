using System.Threading;
using System.Threading.Tasks;
using Dwragge.BlobBlaze.Application.Requests;
using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dwragge.BlobBlaze.Application.Notifications
{
    public class FolderCreatedNotification : INotification
    {
        public FolderCreatedNotification(BackupFolder folder)
        {
            Folder = folder;
        }

        public BackupFolder Folder { get; set; }
    }

    public class CalculateNewFolderSize : INotificationHandler<FolderCreatedNotification>
    {
        private readonly IDirectoryEnumerator _directoryEnumerator;
        private readonly ILogger _logger;
        private readonly IApplicationContextFactory _contextFactory;

        public CalculateNewFolderSize(IDirectoryEnumerator enumerator, IApplicationContextFactory contextFactory, ILogger<CalculateNewFolderSize> logger)
        {
            _directoryEnumerator = enumerator;
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task Handle(FolderCreatedNotification notification, CancellationToken cancellationToken)
        {
            var folder = notification.Folder;
            _logger.LogInformation("New folder {name} with path {path} created, calculating size...", folder.Name, folder.Path);

            var directorySize = await _directoryEnumerator.GetSize(folder.Path);

            using (var context = _contextFactory.CreateContext())
            {
                context.BackupFolders.Attach(folder);
                folder.Size = directorySize;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }

    public class CreateSyncJobForNewFolder : INotificationHandler<FolderCreatedNotification>
    {
        private readonly IMediator _mediatr;
        private readonly ILogger _logger;
        public CreateSyncJobForNewFolder(IMediator mediatr, ILogger<CreateSyncJobForNewFolder> logger)
        {
            _mediatr = mediatr;
            _logger = logger;
        }

        public async Task Handle(FolderCreatedNotification notification, CancellationToken cancellationToken)
        {
            await _mediatr.Send(new ScheduleJobForFolderRequest(notification.Folder), cancellationToken);
        }
    }
}
