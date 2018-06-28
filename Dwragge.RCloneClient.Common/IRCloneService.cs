using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dwragge.RCloneClient.Common
{
    public interface IRCloneService
    {
        Task ExecuteCommand(string commandString, Action<string> outputReceivedAction);
        Task<IEnumerable<string>> GetRemotes();
    }
}