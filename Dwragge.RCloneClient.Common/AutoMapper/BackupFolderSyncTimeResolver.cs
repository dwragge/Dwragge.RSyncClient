using AutoMapper;
using Dwragge.RCloneClient.Persistence;

namespace Dwragge.RCloneClient.Common.AutoMapper
{
    public class BackupFolderSyncTimeResolver : IValueResolver<BackupFolderDto, BackupFolderInfo, TimeValue>
    {
        public TimeValue Resolve(BackupFolderDto source, BackupFolderInfo destination, TimeValue destMember,
            ResolutionContext context)
        {
            return new TimeValue(source.SyncTimeHour, source.SyncTimeMinute);
        }
    }
}
