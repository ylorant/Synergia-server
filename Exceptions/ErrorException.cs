using System;

namespace MMORPGServer
{
	public class ErrorException : ApplicationException
	{
		public ClientError ErrNo { get; private set; }
		public ErrorException() : base("Unknown error")
		{
			this.ErrNo = ClientError.NONE;
		}
		
		public ErrorException(ClientError Errno) : base("An error occured")
		{
			this.ErrNo = Errno;
		}
		
		public ErrorException(ClientError Errno, string message) : base(message)
		{
			this.ErrNo = Errno;
		}
	}
}

