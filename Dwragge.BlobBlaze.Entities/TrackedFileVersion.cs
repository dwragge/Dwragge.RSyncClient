using System;
using System.ComponentModel.DataAnnotations;

namespace Dwragge.BlobBlaze.Entities
{
    public class TrackedFileVersion
    {
        public TrackedFileVersion(TrackedFile file)
        {
            TrackedFileId = file.TrackedFileId;
            RemoteLocation = file.RemoteLocation;
            VersionedAt = DateTime.UtcNow;
        }

        private TrackedFileVersion() { }

        [Key]
        public int TrackedFileVersionId { get; set; }
        public int TrackedFileId { get; private set; }
        public virtual TrackedFile File { get; set; }
        public string RemoteLocation { get; private set; }
        public DateTime VersionedAt { get; private set; }
    }
}
