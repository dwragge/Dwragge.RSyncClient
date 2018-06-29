using System.ComponentModel.DataAnnotations;

namespace Dwragge.RCloneClient.Persistence
{
    public class PendingFileDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public int BackupFolderId { get; set; }

        [Required]
        public string RemoteLocation { get; set; }

        public virtual BackupFolderDto BackupFolder { get; set; }
    }
}
