using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Dwragge.BlobBlaze.Entities
{
    public class BackupRemote
    {
        private string _name;
        private string _baseFolder;
        private string _connectionString;

        public BackupRemote(string name, string baseFolder, string connectionString)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BaseFolder = baseFolder ?? throw new ArgumentNullException(nameof(baseFolder));
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        [Key]
        public int RemoteId { get; set; }
        public string Name
        {
            get => _name;
            set
            {
                if (value.Length > 50) throw new ArgumentOutOfRangeException(nameof(value));
                _name = value;
            }
        }

        public string BaseFolder
        {
            get => _baseFolder;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException(nameof(value), $"BaseFolder (Container Name) can't be empty");
                }
                try
                {
                    var _ = Path.GetFullPath(value);
                }
                catch (IOException)
                {
                    throw new ArgumentException(nameof(value), $"{value} is not a valid path string.");
                }

                _baseFolder = value;
            }
        }

        public string ConnectionString
        {
            get => _connectionString;
            set
            {
                if (value == "development")
                {
                    _connectionString = value;
                    return;
                }

                var splits = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length < 2 || splits.Length > 3)
                {
                    throw new ArgumentException(nameof(value));
                }

                string protocol = "https", account = null, key = null, suffix = "core.windows.net";
                foreach (var split in splits)
                {
                    if (split.StartsWith("DefaultEndpointsProtocol=", StringComparison.OrdinalIgnoreCase))
                    {
                        protocol = split.Substring(25);
                    }
                    else if (split.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))
                    {
                        account = split.Substring(11);
                    }
                    else if (split.StartsWith("AccountKey=", StringComparison.OrdinalIgnoreCase))
                    {
                        key = split.Substring(10);
                    }
                    else if (split.StartsWith("EndpointSuffix=", StringComparison.OrdinalIgnoreCase))
                    {
                        suffix = split.Substring(14);
                    }
                }

                if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(key))
                {
                    throw new ArgumentException(nameof(value));
                }


                _connectionString = $"DefaultEndpointsProtocol={protocol};AccountName={account};AccountKey={key};EndpointSuffix={suffix}";
            }
        }
    }
}
