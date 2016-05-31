using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RemoteCMDClient;

namespace RemoteCMDClient
{
	class Program
	{
		static void Main(string[] args)
		{
			Client client = new Client("127.0.0.1", 10434);
			Console.ReadLine();
		}
	}
}
