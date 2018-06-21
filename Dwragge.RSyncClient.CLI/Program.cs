using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dwragge.RCloneClient.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ServiceClient.ServiceClient();
            //var client = new ServiceProxy();
            Console.WriteLine(client.HelloWorld());
            Console.ReadKey();
        }
    }
}
