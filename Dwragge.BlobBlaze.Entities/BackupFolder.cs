using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Dwragge.BlobBlaze.Entities
{
    public class BackupFolder
    {
        private string _path;
        private string _name;
        private TimeSpan _syncTimeSpan = TimeSpan.FromDays(1);
        private string _remoteBaseFolder = "";

        [Key]
        public int BackupFolderId { get; set; } 

        public BackupFolder(string path, BackupRemote remote)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            BackupRemoteId = remote.BackupRemoteId;
            Remote = remote;
        }

        //Entity Framework Constructor
        private BackupFolder()
        {
        }

        public string Path
        {
            get => _path;
            set
            {
                if (!Directory.Exists(value)) throw new ArgumentException($"Directory {value} doesn't exist or is inaccessible.");
                _path = value;
            }
        }

        public bool RealTimeUpdates { get; set; }

        public string RemoteBaseFolder
        {
            get => _remoteBaseFolder;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(nameof(value), $"BaseFolder (Container Name) can't be empty");
                }

                try
                {
                    var _ = System.IO.Path.GetFullPath(value);
                }
                catch (IOException)
                {
                    throw new ArgumentException(nameof(value), $"{value} is not a valid path string.");
                }

                _remoteBaseFolder = value;
            }
        }

        public TimeSpan SyncTimeSpan
        {
            get => _syncTimeSpan;
            set
            {
                if (Math.Abs(SyncTimeSpan.TotalDays % 1) > double.Epsilon * 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Sync Time Span must be whole days only");
                }

                if (SyncTimeSpan.Days < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Sync Time Span must be at least 1 day");
                }

                _syncTimeSpan = value;
            }
        }

        public TimeValue SyncTime { get; set; } = new TimeValue(2, 0, 0); // Default 02:00AM

        public DateTime? LastSync { get; }

        public string Name
        {
            get => string.IsNullOrEmpty(_name) ? Path : _name;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                _name = value;
            }
        }

        public int BackupRemoteId { get; set; }
        public virtual BackupRemote Remote { get; set; }
    }
}
