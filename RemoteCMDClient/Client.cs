using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RemoteCMDClient
{
    class Client
    {
        private TcpClient _tcpClient;
		private NegotiateStream kerb;

		public Client(string host, int ip)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(host, ip);
            AcceptCallback();
        }

        private void AcceptCallback()
        {
            try
            {
				using (var tcpStream = _tcpClient.GetStream())
				{
					kerb = new NegotiateStream(tcpStream);
					Console.Write("Username: ");
					var user = Console.ReadLine().Split('\\');
					Console.Write("Password: ");
					var password = Console.ReadLine();
					Console.Write("SPN: ");
					var spn = Console.ReadLine();

					NetworkCredential cred;
					if (user.Length == 1) 
						cred = new NetworkCredential(user[0], password, "");
					else
						cred = new NetworkCredential(user[1], password, user[0]);

					kerb.AuthenticateAsClient(
						cred,
						spn,
						ProtectionLevel.EncryptAndSign,
						System.Security.Principal.TokenImpersonationLevel.Impersonation
					);

					using (var br = new BinaryReader(kerb, Encoding.Unicode))
					using (var bw = new BinaryWriter(kerb, Encoding.Unicode))
					{
						br.ReadString();
						br.ReadString();
						Task task = new Task(() =>
							{
								while (true)
									Console.WriteLine(br.ReadString());
							}
						);
						task.Start();

						while (true)
						{
							Console.Write("CMD command: ");
							string command = Console.ReadLine();
							bw.Write(command);
							if (command == "exit!")
								break;
							Console.WriteLine("Execute results:");
						}
					}
				}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
