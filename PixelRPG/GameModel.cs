using System;

namespace PixelRPG
{
	public class GameModel
	{
		public Player Player { get; private set; }
		public WorldElement[,] World { get; private set; }
		public Dictionary<Point,Mob> Mobs { get; private set; }
		public readonly WorldElement OutOfBounds = new WorldElement("OutOfBounds", false, int.MaxValue);
		private const double emptyPercent = 3d / 4;
		private const int MobCount = 25;
		public readonly List<WorldElement> NatureWorldElementsList;
		public readonly Dictionary<Craft, WorldElement[,]> Crafts2by2;
		public readonly List<Mob> NatureMobsPrototypes;
		public readonly Dictionary<string, Mob> AllMobsPrototypes = new Dictionary<string, Mob>()
		{
			{"Chicken",new Mob("Chicken",MobActionType.Peaceful,20,new Point(-1,-1)) }
		};
		public readonly Dictionary<string, WorldElement> AllWorldElements = new Dictionary<string, WorldElement>()
		{
			{"OutOfBounds",new WorldElement("OutOfBounds", false, int.MaxValue) },
			{"Empty",new WorldElement("Empty",false,int.MaxValue,true) },
			{"Grass", new WorldElement("Grass",false,0,true,0,new WorldElement("Turf",true,int.MaxValue))},
			{"Turf",new WorldElement("Turf",true,int.MaxValue) },
			{"Tree",  new WorldElement("Tree",false,1,false,0,new WorldElement("Wood",true,int.MaxValue))},
			{"Wood", new WorldElement("Wood",true,int.MaxValue)},
			{"Stone",  new WorldElement("Stone",true,int.MaxValue)},
			{"Bush",new WorldElement("Bush",false,0,false,0,new WorldElement("Stick",true,int.MaxValue)) },
			{"Stick", new WorldElement("Stick",true,int.MaxValue)},
			{"StoneChopper",new WorldElement("StoneChopper",true,int.MaxValue,true,1) }
        };
		public GameModel(int worldSize)
		{
            Crafts2by2 = GetCrafts();
            NatureWorldElementsList = GetNatureWorldElementList();
            Player = new Player(Characters.Base,new Point(7,13),Sides.Down);
			World = CreateWorld(worldSize);
			NatureMobsPrototypes = GetNatureMobs();
			Mobs = GetWorldMobs();
		}

		public List<Mob> GetNatureMobs()
		{
			return new List<Mob>()
			{
				AllMobsPrototypes["Chicken"]
			};
		}

		public Dictionary<Point, Mob> GetWorldMobs()
		{
			var spavnedMobsCount = 0;
			var random = new Random();
			var mobs = new Dictionary<Point, Mob>();
			do
			{
				var x = random.Next(0,World.GetLength(0));
				var y = random.Next(0, World.GetLength(1));
				if (World[x,y].Name=="Empty")
				{
					var mobPrototype = NatureMobsPrototypes[random.Next(0, NatureMobsPrototypes.Count)];
					mobs[new Point(x,y)]=new Mob(mobPrototype.Name,mobPrototype.Action,mobPrototype.Health,new Point(x,y));
					spavnedMobsCount++;
				}
			} while (spavnedMobsCount < MobCount);
			return mobs;
		}

		public List<WorldElement> GetNatureWorldElementList()
		{
			return new List<WorldElement>()
			{
			    AllWorldElements["Empty"],
			    AllWorldElements["Grass"],
			    AllWorldElements["Tree"],
			    AllWorldElements["Stone"],
			    AllWorldElements["Bush"]
			};
        }

        public Dictionary<Craft, WorldElement[,]> GetCrafts()
		{
			return new Dictionary<Craft, WorldElement[,]>()
			{ 
				{
					new Craft( new WorldElement[2,2]
					{
						{ AllWorldElements["Stone"], AllWorldElements["Empty"] },
						{ AllWorldElements["Stick"],AllWorldElements["Empty"] }
					}),
					new WorldElement[1,2]
					{
						{AllWorldElements["StoneChopper"], AllWorldElements["Empty"] }
					}
				}
			};
        }

		public WorldElement[,] CreateWorld(int worldSize)
		{
			var random = new Random();
			var world = new WorldElement[worldSize, worldSize];
			for (int i = 0; i < worldSize; i++)
				for (int j = 0; j < worldSize; j++)
				{
					var number = random.NextDouble();
					if (number > emptyPercent)
						world[i, j] = NatureWorldElementsList[random.Next(NatureWorldElementsList.Count)];
					else
						world[i, j] = NatureWorldElementsList[0];
                }
			return world;
		}

		public bool PickItem(Point itemPosition)
		{
			var result = false;
			if (InBounds(itemPosition) && World[itemPosition.X,itemPosition.Y].IsItem)
				if (Player.Inventory.AddInFirstEmptySlot(World[itemPosition.X, itemPosition.Y]))
				{
					World[itemPosition.X, itemPosition.Y] = NatureWorldElementsList[0];
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

        public string FileName(Mob mob) => @"images\mobs\" + mob.Name + ".png";

        public string FileName(Player player) => @"images\characters\"+player.Type.ToString()+player.Direction.ToString()+".png";  
        
    }
}
