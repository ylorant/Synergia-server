using System;
using System.Collections.Generic;
using System.IO;

namespace MMORPGServer
{
	public class Parser
	{
		private Dictionary<string, string> data;
		
		public Parser()
		{
			this.data = new Dictionary<string, string>();
		}
		
		public void Parse(string file)
		{
			StreamReader sr = new StreamReader(file);
			
			string line;
			string[] pair;
			string ns = "";
			while(!sr.EndOfStream)
			{
				line = sr.ReadLine();
				pair = line.Split(";".ToCharArray(), 2);
				line = pair[0];
				
				if(line.Length == 0)
					continue;
				
				if(line[0] == '[')
					ns = line.Substring(1, line.IndexOf("]", 1)-1);
				else
				{
					if(!line.Contains("="))
						this.Set(ns + "." + line, "true");
					else
					{
						pair = line.Split("=".ToCharArray(), 2);
						this.Set(ns + "." + pair[0].Trim(), pair[1].Trim());
					}
				}
			}
		}
		
		public string Get(string key)
		{
			if(this.data.ContainsKey(key))
				return this.data[key];
			else
				throw new MissingConfigParamException(key);
		}
		
		public void Set(string key, string value)
		{
			if(!this.data.ContainsKey(key))
				this.data.Add (key, value);
			else
				this.data[key] = value;
		}
		
		public bool Exists(string key)
		{
			return this.data.ContainsKey(key);
		}
	}
}

