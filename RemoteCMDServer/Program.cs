using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteCMDServer;

namespace RemoteCMDServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(10434);
            Console.ReadLine();
        }
    }
}
