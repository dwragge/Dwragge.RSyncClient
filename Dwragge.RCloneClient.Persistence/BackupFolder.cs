using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Dwragge.RCloneClient.Persistence
{
    public class BackupFolder : IValidatableObject
    {
        private string _name;

        [Key]
        public int Id { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public bool RealTimeUpdates { get; set; }

        [Required]
        public TimeSpan SyncTimeSpan { get; set; } = TimeSpan.FromDays(1);

        public string Name
        {
            get => string.IsNullOrEmpty(_name) ? Path : _name;
            set => _name = value;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Directory.Exists(Path))
            {
                yield return new ValidationResult($"Path {Path} does not exist or you do not have permission to access it.");
            }

            if (Math.Abs(SyncTimeSpan.TotalDays % 1) > double.Epsilon * 100)
            {
                yield return new ValidationResult("Sync Time Span must be whole days only");
            }

            if (SyncTimeSpan.Days < 1)
            {
                yield return new ValidationResult("Sync Time Span must be at least 1 day");
            }
        }
    }
}