using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dwragge.RCloneClient.Common
{
    public interface IRCloneService
    {
        Task ExecuteCommand(string commandString);
        Task<IEnumerable<string>> GetRemotes();
    }
}