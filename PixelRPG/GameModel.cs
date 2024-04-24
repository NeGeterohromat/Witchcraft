using System;

namespace PixelRPG
{
	public class GameModel
	{
		public Player Player { get; private set; }
		public WorldElement[,] World { get; private set; }
		public readonly WorldElement OutOfBounds = new WorldElement("OutOfBounds", false);
		public readonly List<WorldElement> WorldElementsList = new List<WorldElement>()
		{
			new WorldElement("Empty",true),
			new WorldElement("Grass",true),
			new WorldElement("Tree",false)
		};
		public GameModel(int worldSize)
		{
			Player = new Player(Characters.Base,new Point(7,13),Sides.Down);
			World = CreateWorld(worldSize);
		}

		public WorldElement[,] CreateWorld(int worldSize)
		{
			var random = new Random();
			var world = new WorldElement[worldSize, worldSize];
			for (int i = 0; i < worldSize; i++)
				for (int j = 0; j < worldSize; j++)
					world[i, j] = WorldElementsList[random.Next(WorldElementsList.Count)];
			return world;
		}

		public void SetPlayerPosition(Point newPosition)
		{
			if (InBounds(newPosition))
				Player.SetPosition(newPosition);
			else
				throw new ArgumentException();
		}

		public void SetPlayerView(Sides view) => Player.SetDirection(view);

		public bool InBounds(Point point)=>
			point.X >= 0 && point.X < World.GetLength(0) && point.Y >= 0 && point.Y < World.GetLength(1);

		public bool IsStepablePoint(Point point) =>
			World[point.X, point.Y].CanPlayerMoveIn;

        public string FileName(WorldElement cell)  => @"images\world\"+cell.Name+".png";

        public string FileName(Player player) => @"images\characters\"+player.Type.ToString()+player.Direction.ToString()+".png";  
        
    }
}
