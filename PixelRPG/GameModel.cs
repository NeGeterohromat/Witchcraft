using System;

namespace PixelRPG
{
	public class GameModel
	{
		public Player Player { get; private set; }
		public WorldElement[,] World { get; private set; }
		public readonly WorldElement OutOfBounds = new WorldElement("OutOfBounds", false, int.MaxValue);
		private const double emptyCount = 3d / 4;
		public readonly List<WorldElement> WorldElementsList = new List<WorldElement>()
		{
			new WorldElement("Empty",false,int.MaxValue,true),
			new WorldElement("Grass",false,0,true,0,new WorldElement("Turf",true,int.MaxValue)),
			new WorldElement("Tree",false,1,false,0,new WorldElement("Wood",true,int.MaxValue)),
			new WorldElement("Stone",true,int.MaxValue),
			new WorldElement("Bush",false,0,false,0,new WorldElement("Stick",true,int.MaxValue))
		};
		public readonly Dictionary<Craft, WorldElement[,]> Crafts2by2 = new Dictionary<Craft, WorldElement[,]>()
		{
			{
				new Craft( new WorldElement[2,2]
				{
					{ new WorldElement("Stone",true,int.MaxValue), new WorldElement("Empty",false,int.MaxValue,true) },
					{ new WorldElement("Stick",true,int.MaxValue), new WorldElement("Empty",false,int.MaxValue,true) }
				}),
				new WorldElement[1,2]
				{
					{ new WorldElement("StoneChopper",true,int.MaxValue,true,1), new WorldElement("Empty",false,int.MaxValue,true)}
				}
			}
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
				{
					var number = random.NextDouble();
					if (number > emptyCount)
						world[i, j] = WorldElementsList[random.Next(WorldElementsList.Count)];
					else
						world[i, j] = WorldElementsList[0];
                }
			return world;
		}

		public bool PickItem(Point itemPosition)
		{
			var result = false;
			if (InBounds(itemPosition) && World[itemPosition.X,itemPosition.Y].IsItem)
				if (Player.Inventory.AddInFirstEmptySlot(World[itemPosition.X, itemPosition.Y]))
				{
					World[itemPosition.X, itemPosition.Y] = WorldElementsList[0];
					result = true;
                }
			return result;
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
