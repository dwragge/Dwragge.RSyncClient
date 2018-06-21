using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dwragge.RSyncClient.CLI.RCloneManagementServiceClient;

namespace Dwragge.RCloneClient.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new RCloneManagementServiceClient();
            //var client = new ServiceProxy();
            for (int i = 0; i < 3; i++)
            {
                client.PostHelloJob("Dylan");
                Console.WriteLine("Sent");
                Console.ReadKey();
            }
        }
    }
}
