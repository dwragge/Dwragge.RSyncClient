using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dwragge.RCloneClient.Common
{
    public interface IBackedUpFileTracker
    {
        Task TrackNewFilesAsync(IEnumerable<string> newFiles, string parentFolder, string remotePath);
    }
}