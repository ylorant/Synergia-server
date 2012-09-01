using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;

namespace MMORPGServer
{
	public class Client
	{
		private static int lastID = 0;
		private int port;
		
		public int ID;
		public int? ClientID {get; private set;}
		public string Username {get; private set;}
		public string Password {get; private set;}
		public bool MapServer {get; private set;}
		public IPAddress Address {get; private set;}
		public int Port
		{
			get
			{
				return this.port;
			}
			set
			{
				if(value >= 0 && value < 65536)
					this.port = value;
			}
		}
		public Socket Sock {get; protected set;}
		
		public Character Character; //Character associated to the client, used only in the Map Server
		public double LastCommandTime;
		
		public Client()
		{
			this.MapServer = false;
			this.ClientID = null;
//			this.ID = Client.lastID++;
			this.LastCommandTime = Timestamp.Convert(DateTime.Now);
		}
		
		public Client (IPAddress addr, int port, Socket sock)
		{
			this.MapServer = false;
			this.ClientID = null;
			this.ID = Client.lastID++;
			this.LastCommandTime = Timestamp.Convert(DateTime.Now);
			this.Address = addr;
			this.Port = port;
			this.Sock = sock;
		}
		
		public void Send(string message)
		{
			if(this.ID == -1)
				Log.Write("-->[HeadServer] " + message.Replace("\x7F", ", "), Log.ErrorLevel.DEBUG);
			else
				Log.Write("-->[" + this.ID + "] " + message.Replace("\x7F", ", "), Log.ErrorLevel.DEBUG);
			byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message + "\n");
			this.Sock.Send(buffer);
		}
		
		public void Send(ErrorException e)
		{
			this.Send("error:" + (int)e.ErrNo + "\x7F" + e.Message);
		}
		
		public void Send(string command, string[] args)
		{
			string message = command + ":" + string.Join("\x7F", args);
			this.Send(message);
		}
		
		public void Disconnect()
		{
			this.Sock.Close();
		}
		
		new public string ToString()
		{
			return "(" + this.ID + ")" + this.Address + ":" + this.Port;
		}
		
		public List<Character> ListCharacters()
		{
			if(this.ClientID == null)
				return null;
			
			var data = new Hashtable();
			data.Add ("Client", this.ClientID);
			bool verify = Database.Query("SELECT name, id, lastmap FROM characters WHERE client = @Client", data);
			
			if(!verify)
				throw new ErrorException(ClientError.INTERNAL_ERROR, "Internal error");
			
			Hashtable row;
			List<Character> ret = new List<Character>();
			while((row = Database.Fetch()) != null)
			{
				Character ch = new Character((int)row["id"], (string)row["name"], (string)row["lastmap"]);
				ret.Add(ch);
			}
			
			return ret;
		}
		
		public bool Authentify(string username, string passwordHash)
		{
			var data = new Hashtable();
			data.Add("Username", username);
			data.Add("Password",passwordHash);
			bool ret = Database.Query("SELECT username, password, id, mapserver FROM clients WHERE username = @Username AND password = @Password", data);
			
			if(!ret)
				return false;
			
			var row = Database.Fetch();
			Database.CloseQuery();
			
			if(row != null)
			{
				this.ClientID = (int)row["id"];
				this.Username = (string)row["username"];
				this.Password = (string)row["password"];
				this.MapServer = (bool)row["mapserver"];
				return true;
			}
			
			return false;
		}
	}
}

