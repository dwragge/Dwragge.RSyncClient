using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dwragge.RCloneClient.Persistence;
using NLog;

namespace Dwragge.RCloneClient.Common
{
    public class BackedUpFileTracker : IBackedUpFileTracker
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IJobContextFactory _contextFactory;

        public BackedUpFileTracker(IJobContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task TrackNewFilesAsync(IEnumerable<string> newFiles, string parentFolder, string remotePath)
        {
            IEnumerable<string> files = newFiles as string[] ?? newFiles.ToArray();

            _logger.Info($"Tracking {files.Count()} new files...");
            var dtos = files.Select(f => new TrackedFileDto
            {
                FileName = f,
                IsArchived = true,
                ParentFolder = parentFolder,
                RemoteLocation = remotePath
            });

            using (var context = _contextFactory.CreateContext(false))
            {
                await context.TrackedFiles.AddRangeAsync(dtos);
                await context.SaveChangesAsync();
            }
        }
    }
}
