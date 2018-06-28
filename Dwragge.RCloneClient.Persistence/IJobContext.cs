using Microsoft.EntityFrameworkCore;

namespace Dwragge.RCloneClient.Persistence
{
    public interface IJobContext
    {
        DbSet<BackedUpFileDto> BackedUpFiles { get; set; }
        DbSet<BackupFolderDto> BackupFolders { get; set; }
    }
}