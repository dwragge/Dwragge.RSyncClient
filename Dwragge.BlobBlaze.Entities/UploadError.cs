using System;
using System.ComponentModel.DataAnnotations;

namespace Dwragge.BlobBlaze.Entities
{
    public class UploadError
    {
        [Key]
        public int UploadErrorId { get; set; }
        public int BackupFolderJobId { get; set; }
        public string File { get; set; }
        public string ExceptionMessage { get; set; }
        public string InnerExceptionMessage { get; set; }
        public string StackTrace { get; set; }
        public DateTime ErrorTime { get; set; }

        public BackupFolderJob BackupFolderJob { get; set; }

        private UploadError()
        {
        }

        public UploadError(BackupFileUploadJob job, Exception e)
        {
            BackupFolderJobId = job.ParentJobId;
            File = job.LocalFile.FullName;
            ExceptionMessage = e.Message;
            StackTrace = e.StackTrace;
            ErrorTime = DateTime.UtcNow;
            InnerExceptionMessage = e.InnerException?.Message;
        }
    }
}
