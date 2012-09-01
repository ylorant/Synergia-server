using System;
using System.Net.Sockets;
using System.Threading;

namespace MMORPGServer
{
	public class ClientConnector : Client
	{
		private TcpClient Client;
		private Thread read;
		
		public ClientConnector (string address, int port)
		{
			this.Client = new TcpClient(address, port);
			this.read = new Thread(new ThreadStart(this.Read));
			this.Client.Client.SendTimeout = 5;
			this.read.Start();
			this.Sock = this.Client.Client;
			this.ID = -1;
		}
		
		public void Auth(string username, string password)
		{
			this.Send("auth", new string[]{username, password});
		}
		
		public void Close()
		{
			this.read.Abort();
			//this.Send ("quit");
			this.Client.Close();
		}
		
		//Reads data from client socket and calls events. Threaded.
		public void Read()
		{
			byte[] buffer;
			string message;
			
			while(true)
			{
				if(this.Client.Client.Available > 0)
				{
					message = "";
					while(this.Client.Client.Available > 0)
					{
						buffer = new byte[this.Client.Client.Available];
						this.Client.Client.Receive(buffer);
						message += System.Text.Encoding.UTF8.GetString(buffer); 
					}
					
					string[] commands = message.Split ("\n".ToCharArray());
								
					foreach(string msg in commands)
					{
						//Extracting command name, and raising the appropriate event, when the command isn't empty
						message = msg.Trim();
						if(message.Length == 0)
							continue;
						
						string[] command = message.Split(":".ToCharArray(), 2);
						string[] commandParams;
						if(command.Length > 1)
							commandParams = command[1].Split("\x7F".ToCharArray());
						else
							commandParams = new string[0];
						Log.Write("<--[HeadServer] " + message.Replace("\x7F", ", "), Log.ErrorLevel.DEBUG);
						Events.Raise(command[0], this, commandParams);
					}
				}
				Thread.Sleep(10);
			}
		}
	}
}