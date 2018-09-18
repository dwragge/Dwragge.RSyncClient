using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dwragge.BlobBlaze.Entities
{
    public class BackupFolderJob
    {
        public BackupFolderJob(BackupFolder folder)
        {
            Folder = folder ?? throw new ArgumentNullException(nameof(folder));
            BackupFolderId = folder.BackupFolderId;
        }

        private BackupFolderJob()
        {

        }

        [Key]
        public int BackupFolderJobId { get; set; }
        public BackupFolderJobStatus Status { get; set; } = BackupFolderJobStatus.Pending;
        public int NumFiles { get; set; }
        public DateTime Created { get; private set; } = DateTime.UtcNow;
        public int BackupFolderId { get; private set; }

        [ForeignKey("BackupFolderId")]
        public virtual BackupFolder Folder { get; private set; }
    }

    public enum BackupFolderJobStatus
    {
        Pending,
        InProgress,
        Successful,
        CompletedErrors,
        Errored
    }
}
