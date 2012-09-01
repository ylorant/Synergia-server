using System;
using System.Collections.Generic;
using NDesk.Options;
using System.Threading;

namespace MMORPGServer
{
	class MainClass
	{
		private Parser parser;
		private bool stop;
		private Server server;
		
		public static string ServerName = "MMORPG Server";
		
		public MainClass()
		{
			this.stop = false;
			this.parser = new Parser();
		}
			
		public void Init(string[] args)
		{
			
			Log.Instance.Critical += this.CriticalLog;
			
			//CLI arguments declaration
			var opt = new OptionSet()
			{
				{ "c|config=", v => { this.parser.Parse(v); } },
				{ "e|echo", v => { Log.Instance.Echo = true; } },
				{ "v|verbose=", (int v) => { Log.Instance.Verbosity = v; } }
			};
			
			//Parsing CLI arguments, and throwing exception if necessary
			opt.Parse(args);
			
			Log.Write("Server initialization", Log.ErrorLevel.NOTICE);
			
			//Loading server type from config.
			try
			{
				MainClass.ServerName = this.parser.Get("Main.ServerName");
				Log.Write("This server name is: " + MainClass.ServerName, Log.ErrorLevel.DEBUG);
				
				Log.UseFile(this.parser.Get("Main.ServerType") + ".log");
				
				if(this.parser.Get("Main.ServerType") == "HeadServer")
					this.server = new HeadServer(this.parser);
				else
					this.server = new MapServer(this.parser);
			}
			catch(MissingConfigParamException e)
			{
				Log.Write(e.Message, Log.ErrorLevel.CRITICAL);
				Environment.Exit(0);
			}
			catch(InitializationException e)
			{
				Log.Write(e.Message, Log.ErrorLevel.CRITICAL);
				Log.Write ("Unable to initialize the server properly.", Log.ErrorLevel.CRITICAL);
				Environment.Exit(0);
			}
			
			Log.Write("Initialization: OK", Log.ErrorLevel.NOTICE);
		}
		
		//Closes the server and frees everything
		public void Close()
		{
			this.server.Close();
		}
		
		//Runs the server. Blocking
		public void Run()
		{
			this.server.Start();
			while(!this.stop)
			{
				this.server.Loop();
				Thread.Sleep(10);
			}
		}
		
		//Event: Called when a critical error is filled on the log
		public void CriticalLog(string message)
		{
			this.server.Close();
			Environment.Exit(0);
		}
		
		//Main
		public static void Main (string[] args)
		{
			MainClass main = new MainClass();
			
			try
			{
				main.Init(args);
			}
			catch(OptionException e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine("Usage: MMORPGServer.exe [-c <config-file>] [-e] [-v]");
				return;
			}
			
			main.Run();
			main.Close();
		}
	}
}
