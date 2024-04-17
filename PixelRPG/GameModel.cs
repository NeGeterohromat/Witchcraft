using System;

namespace PixelRPG
{
	public class GameModel
	{
		public Point PlayerPosition { get; private set; }
		public Characters PlayerView { get; private set; }
		public WorldCell[,] World { get; private set; }
		public GameModel(int worldSize)
		{
			PlayerPosition = new Point(7, 13);
			PlayerView = Characters.BaseDown;
			World = CreateWorld(worldSize);
		}

		public WorldCell[,] CreateWorld(int worldSize)
		{
			var random = new Random();
			var world = new WorldCell[worldSize, worldSize];
			for (int i = 0; i < worldSize; i++)
				for (int j = 0; j < worldSize; j++)
					world[i, j] = (WorldCell)random.Next(1, Enum.GetNames(typeof(WorldCell)).Length);
			return world;
		}

		public void SetPlayerPosition(Point newPosition)
		{
			if (InBounds(newPosition))
				PlayerPosition = newPosition;
			else
				throw new ArgumentException();
		}

		public void SetPlayerView(Characters view)=>
			PlayerView = view;


		public bool InBounds(Point point)=>
			point.X >= 0 && point.X < World.GetLength(0) && point.Y >= 0 && point.Y < World.GetLength(1);

		public bool IsStepablePoint(Point point) =>
			World[point.X, point.Y] == WorldCell.Empty || World[point.X, point.Y] == WorldCell.Grass;


        public string FileName(WorldCell cell)
		{
			switch (cell)
			{
				case WorldCell.OutOfBounds:
					return @"images\world\OutOfBounds.png";
				case WorldCell.Empty:
					return @"images\world\Empty.png";
                case WorldCell.Grass:
					return @"images\world\Grass.png";
                case WorldCell.Tree:
                    return @"images\world\Tree.png";
            }
			return null;
		}

        public string FileName(Characters character)
        {
			switch (character)
            {
                case Characters.BaseDown:
                    return @"images\characters\BaseDown.png";
                case Characters.BaseUp:
                    return @"images\characters\BaseUp.png";
                case Characters.BaseLeft:
                    return @"images\characters\BaseLeft.png";
                case Characters.BaseRight:
                    return @"images\characters\BaseRight.png";
            }
            return null;
        }
    }
}
