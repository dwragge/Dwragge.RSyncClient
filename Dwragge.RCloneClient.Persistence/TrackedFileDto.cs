using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dwragge.RCloneClient.Persistence
{
    public class TrackedFileDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public DateTime FirstBackedUp { get; set; } = DateTime.Now;

        [Required]
        public DateTime LastModified { get; set; }

        [Required]
        public long SizeBytes { get; set; }

        [Required]
        public int BackupFolderId { get; set; }
        
        public virtual BackupFolderDto BackupFolder { get; set; }
    }
}
