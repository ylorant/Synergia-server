using System;
using System.Data;
using System.Data.Linq;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;

namespace MMORPGServer
{
	public class Database
	{
		private static volatile Database inst;
		private static object syncRoot = new Object();
		
		public MySqlDataReader result;
		
		public MySqlConnection conn { get; private set; }
		
		public static Database Instance
		{
			get
			{
				if(inst == null)
				{
					lock (syncRoot) 
    	        	{
						if(inst == null)
							inst = new Database();
					}
				}
				
				return inst;
			}
		}
		
		public Database ()
		{
			
		}
		
		public static bool Connect(string address, int port, string username, string password, string database)
		{
			string cs = "Server=" + address + ";Port=" + port + ";Database=" + database + ";User ID=" + username + ";Password=" + password + ";Pooling=false;";
			Database inst = Database.Instance;
			
			try
			{
				inst.conn = new MySqlConnection(cs);
				inst.conn.Open ();
			}
			catch(MySqlException e)
			{
				inst.conn.Close();
				Log.Write("Error while connecting to the database: " + e.Message, Log.ErrorLevel.CRITICAL);
				return false;
			}
			
			return true;
		}
		
		public static bool Query(string query, Hashtable data)
		{
			
			MySqlCommand cmd = new MySqlCommand(query, Database.Instance.conn);
			cmd.CommandText = query;
			if(data != null)
			{
				foreach(DictionaryEntry el in data)
					cmd.Parameters.AddWithValue("@" + (string)el.Key, el.Value);
			}
			
			try
			{
				Instance.result = cmd.ExecuteReader();
			}
			catch(MySqlException e)
			{
				Log.Write("Error while performing query: " + e.Message, Log.ErrorLevel.WARNING);
				Log.Write("Query: " + query, Log.ErrorLevel.NOTICE);
				return false;
			}
			
			return true;
		}
		
		public static Hashtable Fetch()
		{
			if(!Instance.result.Read())
			{
				Instance.result.Close();
				return null;
			}
			
			Hashtable data = new Hashtable();
			for(int i = 0; i < Instance.result.FieldCount; i++)
				data.Add(Instance.result.GetName(i), Instance.result[i]);
			
			return data;
		}
		
		public static void CloseQuery()
		{
			Instance.result.Close();
		}
	}
}

