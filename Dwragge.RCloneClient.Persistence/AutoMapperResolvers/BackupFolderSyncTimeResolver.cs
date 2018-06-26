using AutoMapper;
using Dwragge.RCloneClient.Common;

namespace Dwragge.RCloneClient.Persistence.AutoMapperResolvers
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
