using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Dwragge.BlobBlaze.Application
{
    public class DirectoryEnumerator : IDirectoryEnumerator
    {
        private readonly ConcurrentQueue<string> _directories = new ConcurrentQueue<string>();
        private readonly ConcurrentBag<string> _files = new ConcurrentBag<string>();
        private readonly ILogger _logger;

        public DirectoryEnumerator(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetFiles(string directory)
        {
            if (!Directory.Exists(directory)) throw new ArgumentException(nameof(directory), $"Directory {directory} does not exist.");

            _logger.LogInformation($"Beginning enumeration of {directory}");
            _directories.Enqueue(directory);
            var tasks = new List<Task>();

            while (!_directories.IsEmpty)
            {
                for (int i = 0; i < 4; i++)
                {
                    _directories.TryDequeue(out var dir);
                    if (dir == null) break;

                    _logger.LogDebug($"Spawning thread for sub-directory {dir}");
                    tasks.Add(DiscoverDirectory(dir));
                }

                await Task.WhenAll(tasks);
            }
            
            _logger.LogInformation($"Discovered {_files.Count} files.");
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
