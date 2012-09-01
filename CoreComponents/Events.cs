using System;
using System.Reflection;

namespace MMORPGServer
{
	public class Events
	{
		public delegate void EventDelegate(Client c, string[] command);
		
		//Events
		public static event EventDelegate Ping;
		public static event EventDelegate EventNotFound;
		public static event EventDelegate MapServer;
		public static event EventDelegate Quit;
		public static event EventDelegate Auth;
		public static event EventDelegate CharacterList;
		public static event EventDelegate SetCharacter;
		public static event EventDelegate GetMap;
		public static event EventDelegate CharacterInfo;
		public static event EventDelegate GetPosition;
		public static event EventDelegate MoveUp;
		public static event EventDelegate MoveDown;
		public static event EventDelegate MoveLeft;
		public static event EventDelegate MoveRight;
		
		//Raises an event from its name
		public static void Raise(string eventName, Client c, string[] command)
		{
			//Boooh, switch, ugly as hell, but Reflection is a bit too hard to try raising static events dynamically.
			try
			{
				switch(eventName)
				{
				
					case "ping": Events.Ping(c, command); break;
					case "mapserver": Events.MapServer(c, command); break;
					case "exit": case "quit": Events.Quit(c, command); break;
					case "auth": Events.Auth(c, command); break;
					case "character-list": Events.CharacterList(c, command); break;
					case "set-character": Events.SetCharacter(c, command); break;
					case "getmap": Events.GetMap(c, command); break;
					case "get-position": Events.GetPosition(c, command); break;
					case "character-info": Events.CharacterInfo(c, command); break;
					case "ok": break;
					case "pong": break;
					case "server": break;
					case "moveleft": Events.MoveLeft(c, command); break;
					case "moveup": Events.MoveUp(c, command); break;
					case "movedown": Events.MoveDown(c, command); break;
					case "moveright": Events.MoveRight(c, command); break;
					case "error": Log.Write("[" + command[0] + "] " + command[1], Log.ErrorLevel.WARNING); break;
					default: Events.EventNotFound(c, new string[] { eventName }); break;
				}
			}
			catch(Exception e)
			{
				Log.Write("Exception catched: " + e.Message, Log.ErrorLevel.WARNING);
				c.Send(new ErrorException(ClientError.INTERNAL_ERROR, "Internal error"));
			}
		}
	}
}

