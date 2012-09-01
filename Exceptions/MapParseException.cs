using System;
namespace MMORPGServer
{
	public class MapParseException : System.ApplicationException
	{
		public string Argument {get; private set;}
		public MapParseException() : base("Error while parsing map")
		{
			this.Argument = "";
		}
		
		public MapParseException(string message) : base(message)
		{
			
		}
	}
}

