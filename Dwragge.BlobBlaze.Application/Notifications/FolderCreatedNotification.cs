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
