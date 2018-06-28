using System;
using System.IO;

namespace Dwragge.RCloneClient.Common
{
    public class BackupFolderInfo
    {
        private string _path;
        private string _name;
        private TimeSpan _syncTimeSpan = TimeSpan.FromDays(1);
        private string _remoteBaseFolder = "";
        public int Id { get; set; } 

        public BackupFolderInfo(string path, string remoteName, string remoteBaseFolder)
        {
            Path = path;
            RemoteName = remoteName;
            RemoteBaseFolder = remoteBaseFolder;
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

        public string RemoteName { get; set; }

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

        public RCloneCommand SyncCommand =>
            new RCloneCommand(RCloneSubCommand.Sync)
            {
                LocalPath = Path,
                RemoteName = RemoteName,
                RemotePath = System.IO.Path.Combine(RemoteBaseFolder, new DirectoryInfo(Path).Name).Replace("\\", "/"),
                WithDebugLogging = true,
                IsDryRun = true
            };

        public RCloneCommand CopyCommand =>
            new RCloneCommand(RCloneSubCommand.Copy)
            {
                LocalPath = Path,
                RemoteName = RemoteName,
                RemotePath = System.IO.Path.Combine(RemoteBaseFolder, new DirectoryInfo(Path).Name).Replace("\\", "/"),
                WithDebugLogging = true,
                IsDryRun = true
            };
    }
}
