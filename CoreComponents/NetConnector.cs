using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MMORPGServer
{
	public class NetConnector
	{
		private Socket sock; //Server socket
		public List<Client> clients; //Connected clients
		private Thread timeout; //Timeout thread
		private Thread listen; //Listening thread
		private Thread read; //Read thread
		private bool readLock; //Read lock
		
		public delegate void ClientConnectEventHandler(Client c);
		public event ClientConnectEventHandler ClientDisconnect;
		public event ClientConnectEventHandler ClientConnect;
		
		public NetConnector (string addr, int port)
		{
			this.readLock = false;
			//Client list declaration
			this.clients = new List<Client>();
			
			//Socket creation
			this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.sock.Bind(new IPEndPoint(IPAddress.Parse(addr), port));
			this.sock.Listen(10);
			
			Log.Write("Socket bound on " + addr + ":" + port, Log.ErrorLevel.DEBUG);
		}
		
		public void Start()
		{
			//Creating threads for listening on sockets
			this.listen = new Thread(new ThreadStart(this.AcceptConnection));
			this.timeout = new Thread(new ThreadStart(this.CheckTimeout));
			this.read = new Thread(new ThreadStart(this.ReadClientCommands));
			this.listen.Start();
			this.timeout.Start();
			this.read.Start();
		}
		
		public void Close()
		{
			this.timeout.Abort();
			this.listen.Abort();
			this.read.Abort();
			this.sock.Close();
		}
		
		//Waits and accepts a connection from an incoming client. Threaded.
		public void AcceptConnection()
		{
			Log.Write("Now listening for incoming connections.", Log.ErrorLevel.NOTICE);
			while(true)
			{
				Socket s = this.sock.Accept();
				IPEndPoint remoteEndPoint = s.RemoteEndPoint as IPEndPoint;
				Client c = new Client(remoteEndPoint.Address, remoteEndPoint.Port, s);
				if(this.ClientConnect != null)
					this.ClientConnect(c);
				this.clients.Add (c);
				
				c.Send ("server", new string[]{ MainClass.ServerName });
			}
		}
		
		//Broadcast command to all clients
		public void Broadcast(ErrorException e, int[] exclude)
		{
			foreach(Client c in this.clients)
			{
				if(!exclude.Contains(c.ID))
					c.Send(e);
			}
		}
		
		//Broadcast command to all clients
		public void Broadcast(string command, int[] exclude)
		{
			foreach(Client c in this.clients)
			{
				if(!exclude.Contains(c.ID))
					c.Send(command);
			}
		}
		
		//Broadcast command to all clients
		public void Broadcast(string command, string[] args, int[] exclude)
		{
			foreach(Client c in this.clients)
			{
				if(!exclude.Contains(c.ID))
					c.Send(command, args);
			}
		}
		
		//Reads client commands and trigger events with them.
		public void ReadClientCommands()
		{
			List<Socket> readList = new List<Socket>();
			byte[] buffer;
			string message;
			
			while(true)
			{
				if(this.clients.Count > 0) //We read only if we have connected clients
				{
					//Populating select list with client sockets
					readList.Clear();
					for(int i = 0; i < this.clients.Count; i++)
						readList.Add(this.clients[i].Sock);
					
					Socket.Select(readList, null, null, 1000); //Selecting sockets who are willing to send us data
					
					if(readList.Count > 0) //We process the data only if at least one client sent data
					{
						//Reading and treating data for each socket
						foreach(Socket s in readList)
						{
							if(s.Available > 0) //If the client really sent data and has not just disconnected
							{
								this.readLock = true;
								
								//Getting client associated with socket
								Client c = null;
								for(int i = 0; i < this.clients.Count; i++)
								{
									if(this.clients[i].Sock == s)
									{
										c = this.clients[i];
										break;
									}
								}
								
								if(c == null)
								{
									Log.Write("Can't determine the client for socket.", Log.ErrorLevel.WARNING);
									continue;
								}
								
								//Reading data of the client
								message = "";
								while(s.Available > 0)
								{
									buffer = new byte[s.Available];
									s.Receive(buffer);
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
									Log.Write("<--["+ c.ID + "] " + message.Replace("\x7F", ", "), Log.ErrorLevel.DEBUG);
									Events.Raise(command[0], c, commandParams);
									c.LastCommandTime = Timestamp.Convert(DateTime.Now);
								}
								
								this.readLock = false;
							}
						}
					}
				}
				
				Thread.Sleep(10);
			}
		}
		
		//Checks timeouts from clients. Threaded.
		public void CheckTimeout()
		{
			int i;
			Client c;
			
			while(true)
			{
				for(i = 0; i < this.clients.Count; i++)
				{
					
					c = this.clients[i];
					if(c.Sock.Poll(10000, SelectMode.SelectRead) && c.Sock.Available == 0 && !this.readLock)
					{
						this.Disconnect(c, false);
					}
				}
				
				Thread.Sleep(1000);
			}
		}
		
		public void DisconnectAll()
		{
			if(this.clients.Count > 0)
			{
				foreach(Client c in this.clients)
					this.Disconnect(c);
			}
		}
		
		public void Disconnect(Client c, bool notify = true)
		{
			if(notify)
				c.Send("bye");
			Console.WriteLine(c.ToString());
			if(this.ClientDisconnect != null)
				this.ClientDisconnect(c);
			c.Disconnect();
			this.clients.Remove(c);
		}
	}
}

