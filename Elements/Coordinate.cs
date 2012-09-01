using System;
namespace MMORPGServer
{
	public struct Coordinate
	{
		public short X;
		public short Y;
		
		public Coordinate(short x, short y)
		{
			this.X = x;
			this.Y = y;
		}
		
		new public string ToString()
		{
			return "[" + X + ";" + Y + "]";
		}
	}
}

