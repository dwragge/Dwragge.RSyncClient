using System;
using System.Collections.Generic;
using System.IO;

namespace Dwragge.RCloneClient.Common
{
    public class RCloneCommand
    {
        private string _remotePath;
        private string _localPath;
        public RCloneSubCommand SubCommand { get; }

        public string LocalPath
        {
            get => _localPath;
            set
            {
                if (!Directory.Exists(value))
                {
                    throw new ArgumentException($"Directory {value} does not exist.");
                }

                if (value.EndsWith("\\")) value = value.Remove(value.Length - 1, 1);
                _localPath = value;
            }
        }

        public string RemoteName { get; set; }

        public string RemotePath
        {
            get => _remotePath;
            set => _remotePath = value.Replace(" ", "-");
        }

        public bool IsDryRun { get; set; }
        public bool WithDebugLogging { get; set; }

        public RCloneCommand(RCloneSubCommand subCommand)
        {
            SubCommand = subCommand;
        }

        public string CommandString
        {
            get
            {
                var argsList = new List<string>();
                if (IsDryRun) argsList.Add("-n");
                if (WithDebugLogging) argsList.Add("--log-level DEBUG");

                return $"{SubCommand.GetDescription()} \"{LocalPath}\" {RemoteName}:{RemotePath} {string.Join(" ", argsList)}";
            }
        }
    }
}
