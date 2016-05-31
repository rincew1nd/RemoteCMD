using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteCMDServer
{
    class Server
    {
        private TcpListener _tcpListener;

		public Server(int ip)
        {
            _tcpListener = new TcpListener(IPAddress.Any, ip);
            _tcpListener.Start();
			Console.WriteLine("Waiting connection...");
			_tcpListener.BeginAcceptTcpClient(AcceptCallback, null);
		}

        private void AcceptCallback(IAsyncResult AR)
		{
			_tcpListener.BeginAcceptTcpClient(AcceptCallback, null);
			using (var _tcpClient = _tcpListener.EndAcceptTcpClient(AR))
			{
				Console.WriteLine("Client connected!\nAuthenticating…");
				var kerb = new NegotiateStream(_tcpClient.GetStream());
				
				try
				{
					kerb.AuthenticateAsServer(
						CredentialCache.DefaultNetworkCredentials,
						ProtectionLevel.EncryptAndSign,
						TokenImpersonationLevel.Impersonation
					);
					Thread.CurrentPrincipal = new WindowsPrincipal((WindowsIdentity)kerb.RemoteIdentity);

					RunConsole(kerb);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
			}
        }

		[PrincipalPermission(SecurityAction.Demand, Authenticated = true)]
		public void RunConsole(NegotiateStream nStream)
		{
			using (BinaryWriter bw = new BinaryWriter(nStream, Encoding.Unicode))
			{
				Process process = StartConsole(bw); ;

				try
				{
					using (BinaryReader br = new BinaryReader(nStream, Encoding.Unicode))
					{
						while (true)
						{
							var command = br.ReadString();
							if (command.Equals("exit!"))
								break;

							process.StandardInput.WriteLine(command);
						}
					}

				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
					Console.WriteLine("Client disconnected!");
					Console.WriteLine("Waiting connection...");
				}
				finally
				{
					process.CancelOutputRead();
					process.CancelErrorRead();
					process.Close();
					Console.WriteLine("Waiting connection...");
				}
			}
		}

		private void ProcessOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine, BinaryWriter bw)
		{
			try
			{
				if (bw != null)
					bw.Write(outLine.Data);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private void ProcessErrorDataHandler(object sendingProcess, DataReceivedEventArgs outLine, BinaryWriter bw)
		{
			try
			{
				if (bw != null)
					bw.Write("Error: " + outLine.Data);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private Process StartConsole(BinaryWriter bw)
		{
			var process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					UseShellExecute = false,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					FileName = "cmd.exe"
				}
			};
			process.OutputDataReceived += (sender, e) => ProcessOutputDataHandler(sender, e, bw);
			process.ErrorDataReceived += (sender, e) => ProcessErrorDataHandler(sender, e, bw);
			
			process.Start();
			
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			return process;
		}
	}
}
