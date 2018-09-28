using System;

namespace Dwragge.BlobBlaze.Application
{
    public interface IStateRestorer : IDisposable
    {
        void RestoreState();
    }
}