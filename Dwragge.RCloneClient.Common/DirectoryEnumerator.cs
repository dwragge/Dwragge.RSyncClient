using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Dwragge.RCloneClient.Common
{
    public class DirectoryEnumerator
    {
        private readonly ConcurrentQueue<string> _directories = new ConcurrentQueue<string>();
        private readonly ConcurrentBag<string> _files = new ConcurrentBag<string>();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task<IEnumerable<string>> GetFiles(string directory)
        {
            if (!Directory.Exists(directory)) throw new ArgumentException(nameof(directory), $"Directory {directory} does not exist.");

            _logger.Info($"Beginning enumeration of {directory}");
            _directories.Enqueue(directory);
            var tasks = new List<Task>();

            while (!_directories.IsEmpty)
            {
                for (int i = 0; i < 4; i++)
                {
                    _directories.TryDequeue(out var dir);
                    if (dir == null) break;

                    _logger.Debug($"Spawning thread for sub-directory {dir}");
                    tasks.Add(DiscoverDirectory(dir));
                }

                await Task.WhenAll(tasks);
            }
            
            _logger.Info($"Discovered {_files.Count} files.");
            return _files;
        }

        private Task DiscoverDirectory(string dir)
        {
            var files = Directory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly);
            var directories = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly);

            foreach (var d in directories)
            {
                _directories.Enqueue(d);
            }

            foreach (var file in files)
            {
                _files.Add(file);
            }

            return Task.CompletedTask;
        }
    }
}
