using System;
using System.ComponentModel.DataAnnotations;

namespace Dwragge.RCloneClient.Persistence
{
    public class FileVersionHistoryDto
    {
        [Key]
        public int VersionHistoryId { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string RemoteLocation { get; set; }
        [Required]
        public DateTime VersionedOn { get; set; }
        [Required]
        public int BackupFolderId { get; set; }

        public virtual BackupFolderDto BackupFolder { get; set; }
    }
}
