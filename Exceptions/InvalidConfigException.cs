using System;

namespace MMORPGServer
{
	public class InvalidConfigException : System.ApplicationException
	{
		public string Argument {get; private set;}
		public InvalidConfigException() : base("Invalid Config argument")
		{
			this.Argument = "";
		}
		
		public InvalidConfigException(string argument) : base("Invalid Config argument")
		{
			this.Argument = argument;
		}
		
		public InvalidConfigException(string argument, string message) : base(message)
		{
			this.Argument = argument;
		}
	}
}

