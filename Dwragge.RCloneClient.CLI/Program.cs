using System;
using Dwragge.RCloneClient.CLI.Connected_Services.RCloneManagementServiceClient;

namespace Dwragge.RCloneClient.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RCloneManagementServiceClient();

            Console.WriteLine(client.HelloWorld());
            Console.ReadKey();
        }
    }
}
