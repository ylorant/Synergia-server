using System;
namespace MMORPGServer
{
	public enum ClientError
	{
		NONE = 0,
		UNKNOWN_COMMAND = 1,
		INVALID_LOGIN = 2,
		ACCESS_DENIED = 3,
		NOT_AUTHED = 4,
		INTERNAL_ERROR = 5,
		INVALID_COMMAND = 6,
		UNKNOWN_CHARACTER = 7
	}
}

