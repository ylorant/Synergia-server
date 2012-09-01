using System;
using System.IO;

namespace MMORPGServer
{
	public sealed class Log
	{
		public int Verbosity = 1;
		public bool Echo = false;
		public StreamWriter logfile;
		
		private static volatile Log inst;
		private static object syncRoot = new Object();
		
		public enum ErrorLevel{	DEBUG = 4, NOTICE = 3, WARNING = 2, CRITICAL = 1 }
		
		public delegate void LogCallDelegate(string message);
		public event LogCallDelegate Critical;
		public event LogCallDelegate Warning;
		public event LogCallDelegate Notice;
		public event LogCallDelegate Debug;
		
		public static Log Instance
		{
			get
			{
				if(inst == null)
				{
					lock (syncRoot) 
    	        	{
						if(inst == null)
							inst = new Log();
					}
				}
				
				return inst;
			}
		}
		
		public Log()
		{
			this.logfile = new StreamWriter("server.log", true);
			this.WriteLine("-------- Log started: " + DateTime.Now.ToString() + " --------", ErrorLevel.NOTICE);
		}
		
		public static void UseFile (string file)
		{
			Log.Instance.logfile.Close ();
			Log.Instance.logfile = new StreamWriter(file);
			Log.Instance.WriteLine("-------- Log started: " + DateTime.Now.ToString() + " --------", ErrorLevel.NOTICE);
		}
		
		public static void Write(string message, ErrorLevel level)
		{
			Log inst = Log.Instance;
			inst.WriteLine(message, level);
		}
		
		public void WriteLine(string message, ErrorLevel level)
		{
			string line = "-- ";
			
			switch(level)
			{
				case ErrorLevel.NOTICE:
					line += "Notice";
					break;
				case ErrorLevel.WARNING:
					line += "Warning";
					break;
				case ErrorLevel.CRITICAL:
					line += "Critical Error";
					break;
				case ErrorLevel.DEBUG:
					line += "Debug";
					break;
			}
			
			line += " -- " + message;
			
			if(this.Verbosity >= (int)level)
			{
				if(this.Echo)
					Console.WriteLine(line);
				
				this.logfile.WriteLine(line);
				this.logfile.Flush();
			}
			
			if(level == ErrorLevel.CRITICAL)
				this.Critical(message);
		}
		
		public static void Close()
		{
			Log.Instance.CloseLog();
		}
		
		public void CloseLog()
		{
			Log.Write("Closing log: " + DateTime.Now.ToString() + ".", ErrorLevel.NOTICE);
			this.logfile.Close();
		}
	}
}

