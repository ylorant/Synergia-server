using System;
namespace MMORPGServer
{
	public class Chunk
	{
		public Tile[,] Tiles;
		public Coordinate Position;
		
		public Chunk(Coordinate pos, char[,] data)
		{
			this.Position = pos;
			this.Tiles = new Tile[4,4];
			for(int i = 0; i < 4; i++)
			{
				for(int j = 0; j < 4; j++)
					this.Tiles[i,j] = new Tile(data[i,j]);
			}
		}
		
		public string[] ToStringArray()
		{
			string[] ret = new string[18];
			ret[0] = this.Position.X.ToString();
			ret[1] = this.Position.Y.ToString();
			
			for(int i = 0; i < 4; i++)
			{
				for(int j = 0; j < 4; j++)
					ret[2+(4 * i + j)] = ((int)this.Tiles[i, j].Value).ToString() + "/" + (this.Tiles[i, j].Crossable ? "1" : "0");
			}
			
			return ret;
		}
	}
}

