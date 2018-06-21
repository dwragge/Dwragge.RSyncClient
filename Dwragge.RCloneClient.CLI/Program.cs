using System;
using Dwragge.RCloneClient.CLI.Connected_Services.RCloneManagementServiceClient;

namespace Dwragge.RCloneClient.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RCloneManagementServiceClient();

            for (int i = 0; i < 3; i++)
            {
                client.PostHelloJob("Dylan");
                Console.WriteLine("Sent");
                Console.ReadKey();
            }
        }
    }
}
