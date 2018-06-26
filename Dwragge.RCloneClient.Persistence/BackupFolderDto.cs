using System;
using System.ComponentModel.DataAnnotations;

namespace Dwragge.RCloneClient.Persistence
{
    public class BackupFolderDto
    {
        internal BackupFolderDto()
        {
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public bool RealTimeUpdates { get; set; }

        [Required]
        public string RemoteName { get; set; }

        public string RemoteBaseFolder { get; set; }

        [Required]
        public TimeSpan SyncTimeSpan { get; set; }

        [Required]
        public int SyncTimeMinute { get; set; }

        [Required]
        public int SyncTimeHour { get; set; }

        public DateTime? LastSync { get; set; }

        [Required]
        public string Name { get; set; }
    }
}