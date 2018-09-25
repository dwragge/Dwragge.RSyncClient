using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Dwragge.BlobBlaze.Entities
{
    public class BackupFileUploadJob
    {
        [Key]
        public int BackupFileUploadJobId { get; set; }
        public FileInfo LocalFile { get; }
        public int ParentJobId { get; set; }
        public BackupFolderJob ParentJob { get; }
        public BackupFileUploadJobStatus Status { get; set; } = BackupFileUploadJobStatus.Pending;
        public int RetryCount { get; set; }

        public BackupFileUploadJob(BackupFolderJob parentJob, FileInfo localFile)
        {
            ParentJob = parentJob;
            ParentJobId = parentJob.BackupFolderJobId;
            LocalFile = localFile;
        }

        public string UploadPath
        {
            get
            {
                var baseFolderVersioned = ParentJob.Folder.Name + "-" + ParentJob.Created.ToString("yyyyMMdd");
                var parentDiff = LocalFile.FullName.Replace(ParentJob.Folder.Path, "").Remove(0, 1); // remove the leading slash
                return Path.Combine(ParentJob.Folder.Remote.BaseFolder, baseFolderVersioned, parentDiff).Replace('\\', '/');
            }
        }
    }

    public enum BackupFileUploadJobStatus
    {
        Pending,
        InProgress,
        Errored,
        ErroredRetrying,
        Successful
    }
}
