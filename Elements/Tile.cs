using System;

namespace MMORPGServer
{
	public struct Tile
	{
		public char Value;
		public bool Crossable;
		
		public Tile (char value)
		{
			this.Value = value;
			this.Crossable = true;
		}
	}
}

