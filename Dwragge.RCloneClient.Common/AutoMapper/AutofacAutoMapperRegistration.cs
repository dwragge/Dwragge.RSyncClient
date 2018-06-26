using Autofac;
using AutoMapper;
using Dwragge.RCloneClient.Persistence;

namespace Dwragge.RCloneClient.Common.AutoMapper
{
    public static class AutofacAutoMapperRegistration
    {
        public static void RegisterAutoMapper(this ContainerBuilder builder)
        {
            var config = new MapperConfiguration(cfg =>
            {
                

                cfg.CreateMap<BackupFolderDto, BackupFolderInfo>()
                    .ForMember(dest => dest.SyncTime, opt => opt.ResolveUsing<BackupFolderSyncTimeResolver>());

                cfg.CreateMap<BackupFolderInfo, BackupFolderDto>()
                    .ForMember(dest => dest.SyncTimeHour, opt => opt.MapFrom(src => src.SyncTime.Hour))
                    .ForMember(dest => dest.SyncTimeMinute, opt => opt.MapFrom(src => src.SyncTime.Minute));
            });


            builder.Register(c => config)
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.Register(c => c.Resolve<IConfigurationProvider>().CreateMapper()).As<IMapper>();
        }
    }
}
