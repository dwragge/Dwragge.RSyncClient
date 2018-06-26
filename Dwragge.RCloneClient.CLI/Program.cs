using System;
using Autofac;
using AutoMapper;
using Dwragge.RCloneClient.CLI.ServiceClient;
using Dwragge.RCloneClient.Common;
using Dwragge.RCloneClient.Common.AutoMapper;

namespace Dwragge.RCloneClient.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RCloneManagementServiceClient();
            var builder = new ContainerBuilder();
            builder.RegisterAutoMapper();
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var mapper = scope.Resolve<IMapper>();
                mapper.ConfigurationProvider.AssertConfigurationIsValid();
                var folder = new BackupFolderInfo(@"M:\cbr", "azure", "backup")
                {
                    SyncTime = new TimeValue(22, 35)
                };
                var dto = mapper.Map<BackupFolderDto>(folder);
                client.CreateTask(dto);
            }
            
            Console.ReadKey();
            Console.WriteLine(client.HelloWorld());
            Console.ReadKey();
        }
    }
}
