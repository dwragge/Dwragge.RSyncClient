using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dwragge.RCloneClient.Persistence
{
    public class BackedUpFileDto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public DateTime FirstBackedUp { get; set; } = DateTime.Now;

        [Required]
        public DateTime LastModified { get; set; } = DateTime.Now;

        [Required]
        public string ParentFolder { get; set; }

        [Required]
        public string RemoteLocation { get; set; }

        [Required]
        public bool IsArchived { get; set; }
    }
}
