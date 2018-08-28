using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Dwragge.BlobBlaze.Entities
{
    public class TrackedFile
    {
        private string _fileName;

        public TrackedFile(BackupFileUploadJob job)
        {
            if (job.Status != BackupFileUploadJobStatus.Successful) throw new InvalidOperationException("Cannot track a file from an incomplete job");

            BackupFolderId = job.ParentJob.Folder.BackupFolderId;
            _fileName = job.LocalFile.FullName;
            RemoteLocation = job.UploadPath;
            UpdateFromFileInfo(job.LocalFile);
        }

        [Key]
        public int TrackedFileId { get; set; }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (!File.Exists(value)) throw new ArgumentException(nameof(value));
                _fileName = value;
            }
        }

        public string RemoteLocation { get; private set; }
        public DateTime FirstTracked { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; private set; }
        public long SizeBytes { get; private set; }
        public int BackupFolderId { get; private set; }
        public virtual BackupFolder BackupFolder { get; set; }
        public virtual ICollection<TrackedFileVersion> Versions { get; set; }

        public void UpdateFromFileInfo(FileInfo file)
        {
            LastModified = file.LastWriteTimeUtc;
            SizeBytes = file.Length;
        }

        public bool IsOutOfDate
        {
            get
            {
                var localFile = new FileInfo(FileName);
                var sizeMatch = localFile.Length == SizeBytes;
                var dateMatch = Math.Abs((LastModified - localFile.LastWriteTimeUtc).TotalSeconds) < 5;

                return (!sizeMatch || !dateMatch);
            }
        }
    }
}
