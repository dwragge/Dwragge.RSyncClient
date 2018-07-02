using Microsoft.EntityFrameworkCore;

namespace Dwragge.RCloneClient.Persistence
{
    public interface IJobContext
    {
        DbSet<TrackedFileDto> TrackedFiles { get; set; }
        DbSet<BackupFolderDto> BackupFolders { get; set; }
        DbSet<PendingFileDto> PendingFiles { get; set; }
    }
}