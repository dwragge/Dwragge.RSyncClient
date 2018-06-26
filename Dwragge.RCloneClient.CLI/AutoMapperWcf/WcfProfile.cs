using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dwragge.RCloneClient.CLI.ServiceClient;
using Dwragge.RCloneClient.Common;

namespace Dwragge.RCloneClient.CLI.AutoMapperWcf
{
    // ReSharper disable once UnusedMember.Global
    public class WcfProfile : Profile
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
