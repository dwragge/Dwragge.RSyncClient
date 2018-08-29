﻿using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Dwragge.BlobBlaze.Entities
{
    public class BackupRemote
    {
        private string _name;
        private string _baseFolder;

        public BackupRemote(string name, string baseFolder, AzureConnectionString connectionString)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BaseFolder = baseFolder ?? throw new ArgumentNullException(nameof(baseFolder));
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        [Key]
        public int BackupRemoteId { get; set; }
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

        public AzureConnectionString ConnectionString
        {
            get;set;
        }

        public string UrlName => Name.ToLower().Replace(' ', '-');
        public bool Default { get; set; }
    }
}
