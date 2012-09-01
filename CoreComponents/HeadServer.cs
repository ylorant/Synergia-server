using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;

namespace MMORPGServer
{
	public class HeadServer : Server
	{
		private Dictionary<int, MapServerClient>MapClients;
		private Dictionary<Guid, Character> Challenges;
		
		//Constructor. Binds all events and connects to the database.
		public HeadServer (Parser parser) : base(parser)
		{
			Log.Write("Initializing Head Server specific features.", Log.ErrorLevel.DEBUG);
			Log.Write("Connecting to database...", Log.ErrorLevel.NOTICE);
			string DBAddr = parser.Get("Database.Address");
			int DBPort = int.Parse(parser.Get("Database.Port"));
			string DBUsername = parser.Get("Database.Username");
			string DBPassword = parser.Get("Database.Password");
			string DBName = parser.Get("Database.DBName");
			if(!Database.Connect(DBAddr, DBPort, DBUsername, DBPassword, DBName))
				throw new InitializationException("Cannot connect to database");
			
			this.MapClients = new Dictionary<int, MapServerClient>();
			this.Challenges = new Dictionary<Guid, Character>();
			Log.Write("Binding HeadServer related events...", Log.ErrorLevel.DEBUG);
			Events.MapServer += this.MapServerEvent;
			Events.Auth += this.AuthEvent;
			Events.CharacterList += this.CharacterListEvent;
			Events.SetCharacter += this.SetCharacterEvent;
			Events.CharacterInfo += this.CharacterInfoEvent;
			this.NetConnector.ClientDisconnect += this.ClientDisconnect;
		}
		
		public void CharacterInfoEvent(Client c, string[] command)
		{
			if(c.MapServer)
			{
				Guid g;
				if(Guid.TryParse(command[0], out g))
				{
					if(this.Challenges.ContainsKey(g))
					{
						Character ch = this.Challenges[g];
						c.Send("character-info", new string[] { command[0], ch.ID.ToString(), ch.Name });
					}
				}
				else
					c.Send(new ErrorException(ClientError.INTERNAL_ERROR, "Malformed Guid"));
			}
			else
				c.Send(new ErrorException(ClientError.ACCESS_DENIED, "Access denied"));
		}
		
		//MapServer event callback: sets the map provided by the map server sending the command.
		//Note: the client sending this command has to be authed as a map server.
		//Usage: mapserver:<mapid>.<mapname>
		//mapid: Map identifier, alphanumeric.
		//mapname: the name of the map, in human-fancy form.
		public void MapServerEvent(Client c, string[] command)
		{
			if(c.MapServer)
			{
				Log.Write("Client #" + c.ID + " is now defined as a map server, with map '" + command[0] + "'.", Log.ErrorLevel.DEBUG);
				MapServerClient msp = new MapServerClient(c);
				this.MapClients.Add (c.ID, msp);
				msp.MapID = command[0];
				msp.MapName = command[1];
				c.Port = int.Parse(command[2]);
			}
			else
				c.Send(new ErrorException(ClientError.ACCESS_DENIED, "Access denied"));
		}
		
		public void AuthEvent(Client c, string[] command)
		{
			if(!c.Authentify(command[0], command[1]))
				c.Send(new ErrorException(ClientError.INVALID_LOGIN, "Invalid login/password"));
		}
		
		public void CharacterListEvent(Client c, string[] command)
		{
			if(c.ClientID == null)
			{
				c.Send(new ErrorException(ClientError.NOT_AUTHED, "Auth first"));
				return;
			}
			
			c.Send("character-list-begin");
			var chars = c.ListCharacters();
			foreach(Character row in chars)
				c.Send("character", new string[]{row.ID.ToString(), row.Name});
			
			c.Send("character-list-end");
		}
		
		public void SetCharacterEvent(Client c, string[] command)
		{
			var chars = c.ListCharacters();
			int id = int.Parse(command[0]);
			foreach(Character ch in chars)
			{
				if(ch.ID == id)
				{
					MapServerClient msc = this.GetMapServer(ch.LastMap);
					
					if(msc == null)
					{
						c.Send(new ErrorException(ClientError.INTERNAL_ERROR, "Internal error"));
						return;
					}
					
					Guid g = Guid.NewGuid();
					this.Challenges.Add(g, ch);
					c.Send("connect", new string[] { g.ToString(), msc.c.Address.ToString(), msc.c.Port.ToString() });
					return;
				}
			}
				   
			c.Send(new ErrorException(ClientError.UNKNOWN_CHARACTER, "Unknown character"));
		}
		
		public MapServerClient GetMapServer(string mapname)
		{
			foreach(KeyValuePair<int, MapServerClient> msc in this.MapClients)
			{
				if(msc.Value.MapID == mapname)
					return msc.Value;
			}
			
			return null;
		}
		
		public void ClientDisconnect(Client c)
		{
			if(this.MapClients.ContainsKey(c.ID))
			{
				Log.Write("Unregistered MapServer using map "+ this.MapClients[c.ID].MapName + "(" + this.MapClients[c.ID].MapID + ")", Log.ErrorLevel.DEBUG); 
				this.MapClients.Remove(c.ID);
			}
			Log.Write ("Client #" + c.ID + " disconnected.", Log.ErrorLevel.NOTICE);
		}
	}
}

