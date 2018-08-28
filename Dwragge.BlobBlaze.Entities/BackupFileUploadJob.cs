using System.IO;

namespace Dwragge.BlobBlaze.Entities
{
    public class BackupFileUploadJob
    {
        public FileInfo LocalFile { get; }
        public BackupFolderJob ParentJob { get; }
        public BackupFileUploadJobStatus Status { get; set; } = BackupFileUploadJobStatus.Pending;
        public int RetryCount { get; set; }

        public BackupFileUploadJob(BackupFolderJob parentJob, FileInfo localFile)
        {
            ParentJob = parentJob;
            LocalFile = localFile;
        }

        public string UploadPath
        {
            get
            {
                var baseFolderVersioned = ParentJob.Folder.RemoteBaseFolder + "-" + ParentJob.Created.ToString("yyyyMMdd");
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
