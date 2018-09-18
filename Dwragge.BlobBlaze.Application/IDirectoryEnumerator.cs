using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dwragge.BlobBlaze.Application
{
    public interface IDirectoryEnumerator
    {
        Task<IEnumerable<string>> GetFiles(string directory);
        Task<long> GetSize(string directory);
    }
}