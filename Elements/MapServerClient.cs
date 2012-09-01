using System;
using System.Net;
using System.Net.Sockets;

namespace MMORPGServer
{
	public class MapServerClient
	{
		public string MapID;
		public string MapName;
		public Client c;
		
		public MapServerClient (Client c)
		{
			this.c = c;
		}
	}
}

