using System;
using System.Timers;

namespace MMORPGServer
{
	public class Server
	{
		protected NetConnector NetConnector;
		protected Timer PingTimer;
		
		public Server (Parser parser)
		{
			Log.Write("Binding events...", Log.ErrorLevel.DEBUG);
			Events.Ping += this.PingEvent;
			Events.EventNotFound += this.EventNotFound;
			Events.Quit += this.QuitEvent;
			
			Log.Write("Creating NetConnector...", Log.ErrorLevel.DEBUG);
			
			string addr = parser.Get ("Network.Address");
			int port = int.Parse(parser.Get("Network.Port"));
			this.NetConnector = new NetConnector(addr, port);
			this.PingTimer = new Timer(30000);
			this.PingTimer.Elapsed += this.SendPingEvent;
			this.PingTimer.AutoReset = true;
			this.PingTimer.Enabled = true;
			this.PingTimer.Start();
		}
		
		public void Start()
		{
			this.NetConnector.Start();
		}
		
		public void Loop()
		{
			
		}
		
		public void Close()
		{
			this.NetConnector.DisconnectAll();
			this.NetConnector.Close();
		}
		
		//Event binding
		
		//Ping send to all clients
		public void SendPingEvent(object o, ElapsedEventArgs e)
		{
			this.NetConnector.Broadcast("ping", new string[] { Guid.NewGuid().ToString() }, new int[0]);
		}
		
		//Ping, reply pong with correct challenge
		public void PingEvent(Client c, string[] command)
		{
			c.Send("pong:" + command[0]);
		}
		
		//Client disconnection, we disconnect it
		public void QuitEvent(Client c, string[] command)
		{
			this.NetConnector.Disconnect(c);
		}
		
		//Default event called when a command has not triggered another event
		public void EventNotFound(Client c, string[] command)
		{
			c.Send(new ErrorException(ClientError.UNKNOWN_COMMAND, "Unknown command: " + command[0]));
		}
	}
}

