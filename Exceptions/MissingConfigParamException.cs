using System;

namespace MMORPGServer
{
	public class MissingConfigParamException : ApplicationException
	{
		public string Argument {get; private set;}
		public MissingConfigParamException() : base("Missing Config argument")
		{
			this.Argument = "";
		}
		
		public MissingConfigParamException(string argument) : base("Missing Config argument: " + argument)
		{
			this.Argument = argument;
		}
		
		public MissingConfigParamException(string argument, string message) : base(message)
		{
			this.Argument = argument;
		}
	}
}

