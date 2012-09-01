using System;
using System.Collections.Generic;
using System.IO;

namespace MMORPGServer
{
	public class Map
	{
		private Dictionary<Coordinate, Chunk> Chunks;
		private Dictionary<Coordinate, string> JumpPoints;
		public string ID;
		public string Name;
		public string Tileset;
		public Coordinate StartPoint;
		
		//Creates a new empty map
		public Map ()
		{
			this.Chunks = new Dictionary<Coordinate, Chunk>();
			this.JumpPoints = new Dictionary<Coordinate, string>();
		}
		
		//Loads map from a file
		public Map(string mapfile, string jumpfile, string  specfile)
		{
			this.Chunks = new Dictionary<Coordinate, Chunk>();
			this.JumpPoints = new Dictionary<Coordinate, string>();
			this.Load(mapfile, jumpfile, specfile);
		}
		
		//Returns a chunk of the map
		public Chunk GetChunk(Coordinate c)
		{
			if(this.Chunks.ContainsKey(c))
				return this.Chunks[c];
			else
				return null;
		}
		
		//Loads a map
		public void Load(string mapfile, string jumpfile, string specfile)
		{
			FileStream fp;
			BinaryReader rdmap;
			try
			{
				fp = new FileStream(mapfile, FileMode.Open);
				rdmap = new BinaryReader(fp);
			}
			catch(FileNotFoundException e)
			{
				throw new MapParseException(e.Message);
			}
			
			try
			{
				int i = 0;
				
				//Reading the file
				while(rdmap.PeekChar() != -1)
				{
					//Position
					Coordinate c = new Coordinate();
					c.X = rdmap.ReadInt16();
					c.Y = rdmap.ReadInt16();
					
					char[] chunkData = rdmap.ReadChars(16);
					if(chunkData.Length < 16)
						throw new MapParseException("Unexpected end of stream: " + chunkData.Length);
					
					char[,] chunk = new char[4,4];
					int x = 0, y = 0;
					foreach(char cell in chunkData)
					{
						chunk[y,x] = cell;
						x++;
						if(x == 4)
						{
							x = 0;
							y++;
						}
					}
					
					Chunk chunkStruct = new Chunk(c, chunk);
					this.Chunks.Add(c, chunkStruct);
					i++;
				}
				Log.Write("Loaded " + i + " chunks", Log.ErrorLevel.DEBUG);
			}
			catch(IOException e)
			{
				throw new MapParseException(e.Message);
			}
		
			Parser p = new Parser();
			p.Parse(specfile);
			
			this.StartPoint = new Coordinate();
			this.StartPoint.X = short.Parse(p.Get("Spec.StartX"));
			this.StartPoint.Y = short.Parse(p.Get("Spec.StartY"));
		}
	}
}

