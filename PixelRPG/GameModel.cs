using System;

namespace PixelRPG
{
	public class GameModel
	{
		public Point PlayerPosition { get; private set; }
		public WorldCell[,] World { get; private set; }
		public GameModel(int worldSize)
		{
			PlayerPosition = new Point(7, 13);
			World = CreateWorld(worldSize);
		}

		public WorldCell[,] CreateWorld(int worldSize)
		{
			var random = new Random();
			var world = new WorldCell[worldSize, worldSize];
			for (int i = 0; i < worldSize; i++)
				for (int j = 0; j < worldSize; j++)
					world[i, j] = (WorldCell)random.Next(2, Enum.GetNames(typeof(WorldCell)).Length);
			return world;
		}

		public void SetPlayerPosition(Point newPosition)
		{
			if (InBounds(newPosition))
				PlayerPosition = newPosition;
			else
				throw new ArgumentException();
		}


		public bool InBounds(Point point)=>
			point.X >= 0 && point.X < World.GetLength(0) && point.Y >= 0 && point.Y < World.GetLength(1);
		

		public string FileName(WorldCell cell)
		{
			switch (cell)
			{
				case WorldCell.OutOfBounds:
					return @"images\world\OutOfBounds.png";
				case WorldCell.Empty:
					return @"images\world\Transparent.png";
                case WorldCell.Grass:
					return @"images\world\grass.png";
                case WorldCell.Character:
                    return @"images\characters\mainCharacter.png";
            }
			return null;
		}
	}
}
