using System.Threading;
using System.Threading.Tasks;
using Dwragge.BlobBlaze.Application;
using Dwragge.BlobBlaze.Entities;
using Dwragge.BlobBlaze.Storage;
using MediatR;

namespace Dwragge.BlobBlaze.Web.Notifications
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
        private readonly IApplicationContextFactory _contextFactory;

        public CalculateNewFolderSize(IDirectoryEnumerator enumerator, IApplicationContextFactory contextFactory)
        {
            _directoryEnumerator = enumerator;
            _contextFactory = contextFactory;
        }

        public async Task Handle(FolderCreatedNotification notification, CancellationToken cancellationToken)
        {
            var folder = notification.Folder;
            var directorySize = await _directoryEnumerator.GetSize(folder.Path);

            using (var context = _contextFactory.CreateContext())
            {
                var dbFolder = context.BackupFolders.Find(folder.BackupFolderId);
                dbFolder.Size = directorySize;
                await context.SaveChangesAsync();
            }
        }
    }
}
