using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace Dwragge.BlobBlaze.Entities
{
    public class BackupFolderJob
    {
        private int _numComplete = 0;
        private int _numErrored = 0;
        private object _lock = new object();
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

        public void IncrementComplete()
        {
            lock (_lock)
            {
                _numComplete++;
                if (_numComplete + _numErrored == NumFiles)
                {
                    Status = _numErrored > 0 ? BackupFolderJobStatus.CompletedErrors : BackupFolderJobStatus.Successful;
                }
            }
        }

        public void IncrementErrored()
        {
            lock (_lock)
            {
                _numErrored++;
                if (_numComplete + _numErrored == NumFiles)
                {
                    Status =BackupFolderJobStatus.CompletedErrors;
                }
            }
        }
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
