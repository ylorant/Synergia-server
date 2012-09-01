using System;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Timers;
using System.Threading;

namespace MMORPGServer
{
	public class MapServer : Server
	{
		private ClientConnector Client;
		private Map Map;
		private Dictionary<Guid, Client> challengesRequests;
		
		//Constructor. Binds events and connects client socket to the head server.
		public MapServer (Parser parser) : base(parser)
		{
			Log.Write("Initializing Map Server specific features.", Log.ErrorLevel.DEBUG);
			string address = parser.Get("Network.HeadServer.Address");
			int port = int.Parse(parser.Get("Network.HeadServer.Port"));
			string username = parser.Get("Network.HeadServer.Login");
			string password = parser.Get("Network.HeadServer.Password");
			
			this.challengesRequests = new Dictionary<Guid, Client>();
			
			Log.Write("Binding Map server specific events", Log.ErrorLevel.DEBUG);
			Events.GetMap += this.GetMapEvent;
			Events.Auth += this.AuthEvent;
			Events.CharacterInfo += this.CharacterInfoEvent;
			Events.GetPosition += this.GetPositionEvent;
			Events.MoveUp += this.MoveUpEvent;
			Events.MoveDown += this.MoveDownEvent;
			Events.MoveLeft += this.MoveLeftEvent;
			Events.MoveRight += this.MoveRightEvent;
			this.PingTimer.Elapsed += this.SendPingEvent;
			
			this.NetConnector.ClientConnect += this.ClientConnect;
			this.NetConnector.ClientDisconnect += this.ClientDisconnect;
			
			try
			{
				this.Map = new Map(parser.Get("Map.MapFile"), parser.Get("Map.JumpPointFile"), parser.Get("Map.SpecFile"));
				this.Map.Tileset = parser.Get("Map.Tileset");
				this.Map.ID = parser.Get("Game.Map.ID");
				this.Map.Name = parser.Get("Game.Map.Name");
			}
			catch(MapParseException e)
			{
				Log.Write("Error while parsing map: " + e.Message, Log.ErrorLevel.CRITICAL);
				throw new InitializationException("Can't load map");
			}
			
			Log.Write("Connecting to the Head Server", Log.ErrorLevel.NOTICE);
			this.Client = new ClientConnector(address, port);
			this.Client.Auth(username, password);
			this.Client.Send("mapserver" , new string[]{ this.Map.ID, this.Map.Name, parser.Get("Network.Port") });
		}
		
		public void CharacterInfoEvent(Client c, string[] command)
		{
			Guid g;
			if(!Guid.TryParse(command[0],out g))
				c.Send(new ErrorException(ClientError.INTERNAL_ERROR, "Invalid GUID"));
			else if(this.challengesRequests.ContainsKey(g))
			{
				Client cl = this.challengesRequests[g];
				cl.Character = new Character(int.Parse(command[1]), command[2], this.Map.ID);
				cl.Character.Position.X = this.Map.StartPoint.X;
				cl.Character.Position.Y = this.Map.StartPoint.Y;
				cl.Send("player-character", new string[] { cl.Character.ID.ToString(), command[2] });
				this.challengesRequests.Remove(g);
				
				//Aknowledge the other clients of this one
				foreach(Client oc in this.NetConnector.clients)
				{
					if(oc.ID != cl.ID)
					{
						oc.Send("character", new string[] { cl.Character.ID.ToString(), cl.Character.Name, cl.Character.Position.X.ToString(), cl.Character.Position.Y.ToString() });
						cl.Send("character", new string[] { oc.Character.ID.ToString(), oc.Character.Name, oc.Character.Position.X.ToString(), oc.Character.Position.Y.ToString() });
					}
				}
			}
		}
		
		//Callback called when a client connects
		public void ClientConnect(Client c)
		{
			Log.Write("Client connected: #" + c.ID.ToString(), Log.ErrorLevel.DEBUG);
			//c.Character = new Character();
		}
		
		public new void Close()
		{
			Log.Write("Closing server...", Log.ErrorLevel.DEBUG);
			base.Close();
			this.Client.Close();
		}
		
		//Callback called when a client disconnects
		public void ClientDisconnect(Client c)
		{
			Log.Write ("Client #" + c.ID + " disconnected.", Log.ErrorLevel.NOTICE);
			if(c.Character != null)
			{
				foreach(Client cl in this.NetConnector.clients)
				{
					if(cl.ID != c.ID)
						cl.Send("character-out", new string[] { c.Character.ID.ToString()});
				}
			}
		}
		
		//GetMap event callback : returns a fragment of the map to the client
		//Usage: getmap
		public void GetMapEvent(Client c, string[] command)
		{
			
			c.Send("change-tileset", new string[] { this.Map.Tileset });
			short startX = c.Character.Position.X;
			short startY = c.Character.Position.Y;
			short t;
			for(t = 4; t*4 < startX; t += 4);
			startX = t;
			for(t = 4; t*4 < startY; t += 4);
			startY = t;
			startX -= 16;
			startY -= 16;
			Chunk ch;
			
			for(short i = 0; i < 32; i+=4)
			{
				for(short j = 0; j < 32; j += 4)
				{
					if((ch = this.Map.GetChunk(new Coordinate((short)(startX + i), (short)(startY + j)))) != null)
					{
						c.Send("chunk", ch.ToStringArray());
						Thread.Sleep(5);
					}
				}
			}
		}
		
		public void GetPositionEvent(Client c, string[] command)
		{
			
			c.Send("position", new string[] { c.Character.ID.ToString(), c.Character.Position.X.ToString(), c.Character.Position.Y.ToString() });
		}
		
		public void MoveUpEvent(Client c, string[] command)
		{
			if(c.Character.Position.Y > 0)
			{
				this.NetConnector.Broadcast("moveup", new string[] { c.ID.ToString() }, new int[] { c.ID });
				c.Character.Position.Y--;
			}
			else
				this.GetPositionEvent(c, command);
		}
		
		public void MoveDownEvent(Client c, string[] command)
		{
			c.Character.Position.Y++;
			this.NetConnector.Broadcast("movedown", new string[] { c.ID.ToString() }, new int[] { c.ID });
		}
		
		public void MoveLeftEvent(Client c, string[] command)
		{
			if(c.Character.Position.X > 0)
			{
				this.NetConnector.Broadcast("moveleft", new string[] { c.ID.ToString() }, new int[] { c.ID });
				c.Character.Position.X--;
			}
			else
				this.GetPositionEvent(c, command);
		}
		
		public void MoveRightEvent(Client c, string[] command)
		{
			c.Character.Position.X++;
			this.NetConnector.Broadcast("moveright", new string[] { c.ID.ToString() }, new int[] { c.ID });
		}
		
		
		public new void SendPingEvent(object o, ElapsedEventArgs e)
		{
			this.Client.Send("ping", new string[]{ Guid.NewGuid().ToString() });
		}
		
		//Auth event callback : Authenticates the client from the data of the Head server, by a challenge check (GUID).
		//Usage: auth:<challenge>
		public void AuthEvent(Client c, string[] command)
		{
			Guid g;
			if(Guid.TryParse(command[0], out g))
			{
				this.challengesRequests.Add(g, c);
				this.Client.Send("character-info", command);
			}
			else
				c.Send(new ErrorException(ClientError.INTERNAL_ERROR, "Malformed Guid"));
		}
	}
}

