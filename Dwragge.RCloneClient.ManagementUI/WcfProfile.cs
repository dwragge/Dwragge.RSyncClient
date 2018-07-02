using AutoMapper;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.Common.AutoMapper;
using Dwragge.RCloneClient.ManagementUI.ServiceClient;

namespace Dwragge.RCloneClient.ManagementUI
{
    // ReSharper disable once UnusedMember.Global
    public class WcfProfile : Profile, IUserAutomapperProfile
    {
        public WcfProfile()
        {
            CreateMap<BackupFolderInfo, BackupFolderDto>()
                .ForMember(dest => dest.SyncTimeHour, opt => opt.MapFrom(src => src.SyncTime.Hour))
                .ForMember(dest => dest.SyncTimeMinute, opt => opt.MapFrom(src => src.SyncTime.Minute))
                .ForMember(dest => dest.ExtensionData, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
