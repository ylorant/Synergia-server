using System;
namespace MMORPGServer
{
	public class InitializationException : ApplicationException
	{
		public InitializationException () : base()
		{
			
		}
		
		public InitializationException(string message) : base(message)
		{
			
		}
	}
}

