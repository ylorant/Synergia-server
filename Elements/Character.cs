using System;

namespace MMORPGServer
{
	public class Character
	{
		public Coordinate Position;
		public string Name;
		public int ID;
		public string LastMap;
		
		public Character()
		{
			this.Position = new Coordinate(0, 0);
		}
		
		public Character(int ID, string name, string lastmap)
		{
			this.ID = ID;
			this.Name = name;
			this.LastMap = lastmap;
		}
		
		public new string ToString()
		{
			return this.Name + ": " + this.Position.ToString();
		}
	}
}

