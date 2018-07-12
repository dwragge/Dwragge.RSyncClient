using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Dwragge.RCloneClient.Persistence
{
    public class RemoteDto
    {
        [Key]
        public int RemoteId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ConnectionString { get; set; }
    }
}
