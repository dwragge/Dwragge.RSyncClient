using System;
using System.Collections.Generic;
using System.IO;

namespace Dwragge.RCloneClient.Common
{
    public class RCloneCommandBuilder
    {
        private readonly RCloneSubCommand _subCommand;
        private string _localPath;
        private string _remoteName;
        private string _remotePath;
        private readonly List<string> _argsList = new List<string>();

        private RCloneCommandBuilder(RCloneSubCommand command)
        {
            _subCommand = command;
        }

        public static RCloneCommandBuilder CreateCommand(RCloneSubCommand command)
        {
            var builder = new RCloneCommandBuilder(command);
            return builder;
        }

        public RCloneCommandBuilder WithLocalPath(string localPath)
        {
            if (!Directory.Exists(localPath))
            {
                throw new ArgumentException($"Directory {localPath} does not exist.");
            }

            if (localPath.EndsWith("\\")) localPath = localPath.Remove(localPath.Length - 1, 1);

            _localPath = localPath;
            return this;
        }

        public RCloneCommandBuilder WithDebugLogging()
        {
            _argsList.Add("--log-level DEBUG");
            return this;
        }

        public RCloneCommandBuilder WithRemote(string remoteName)
        {
            _remoteName = remoteName;
            return this;
        }

        public RCloneCommandBuilder WithRemotePath(string remotePath)
        {
            var escaped = remotePath.Replace(" ", "-");
            _remotePath = escaped;
            return this;
        }

        public RCloneCommandBuilder AsDryRun()
        {
            _argsList.Add("-n");
            return this;
        }

        public string Build()
        {
            return $"{_subCommand.GetDescription()} \"{_localPath}\" {_remoteName}:{_remotePath} {string.Join(" ", _argsList)}";
        }
    }
}