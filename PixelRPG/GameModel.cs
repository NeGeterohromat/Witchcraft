﻿using System;

namespace PixelRPG
{
	public class GameModel
	{
		public Entity Player { get; private set; }
		public WorldElement[,] World { get; private set; }
		public Dictionary<Point,Entity> Mobs { get; private set; }
        public Dictionary<Point, Chest> Chests { get; private set; }
        public readonly WorldElement OutOfBounds = new WorldElement("OutOfBounds", false, int.MaxValue);
		private const double emptyPercent = 3d / 4;
		private const int MobCount = 25;
        public const double peacefulMobMoveChance = 1d / 10;
        public const int peacefulMobMoveTick = 1*1000;
		public const double enemyMobMoveChance = 1d / 2;
        public readonly List<WorldElement> NatureWorldElementsList;
		public readonly Dictionary<Craft, WorldElement[,]> Crafts2by2;
		public readonly List<Entity> NatureMobsPrototypes;
		public readonly Dictionary<string, Entity> AllMobsPrototypes = new Dictionary<string, Entity>()
		{
			{"Player",new Entity("Player",EntityActionType.Player,20,new Point(7,13),Sides.Down,1,new ArmCraft()) },
			{"Chicken",new Entity("Chicken",EntityActionType.Peaceful,10,new Point(-1,-1),Sides.Down,0,new Inventory()) },
			{"Zombie", new Entity("Zombie",EntityActionType.Enemy,20,new Point(-1,-1),Sides.Down,2,new Inventory()) }
		};
		public readonly Dictionary<string, WorldElement> AllWorldElements = new Dictionary<string, WorldElement>()
		{
			{"OutOfBounds",new WorldElement("OutOfBounds", false, int.MaxValue) },
			{"Empty",new WorldElement("Empty",false,int.MaxValue,true) },
			{"Grass", new WorldElement("Grass",false,0,true,false,0,0,new WorldElement("Turf",true,int.MaxValue))},
			{"Turf",new WorldElement("Turf",true,int.MaxValue) },
			{"Tree",  new WorldElement("Tree",false,1,false,false,0,0,new WorldElement("Wood",true,int.MaxValue))},
			{"Wood", new WorldElement("Wood",true,int.MaxValue)},
			{"Stone",  new WorldElement("Stone",true,int.MaxValue)},
			{"Bush",new WorldElement("Bush",false,0,false,false,0,0,new WorldElement("Stick",true,int.MaxValue)) },
			{"Stick", new WorldElement("Stick",true,int.MaxValue)},
			{"StoneChopper",new WorldElement("StoneChopper",true,int.MaxValue,true,false,1,5) },
			{"Heap",new WorldElement("Heap",false,int.MaxValue,false,true) },
			{"RawChicken",new WorldElement("RawChicken",true,int.MaxValue) } 
        };
		public GameModel(int worldSize)
		{
            Crafts2by2 = GetCrafts();
            NatureWorldElementsList = GetNatureWorldElementList();
			Player = AllMobsPrototypes["Player"];
			World = CreateWorld(worldSize);
			NatureMobsPrototypes = GetNatureMobs();
			Mobs = GetWorldMobs();
			Chests = new Dictionary<Point, Chest>();
		}

		public void MoveEntity(Entity entity, Point point)
		{
			if (IsStepablePoint(point))
			{
				Mobs.Remove(entity.Position);
				entity.SetPosition(point);
				Mobs[point] = entity;
				if (!entity.Equals(Player))
					PickItem(entity.Inventory, point);
			}
		}

		public List<Entity> GetNatureMobs()
		{
			return new List<Entity>()
			{
				AllMobsPrototypes["Chicken"],
				AllMobsPrototypes["Zombie"]
			};
		}

		public Dictionary<Point, Entity> GetWorldMobs()
		{
			var spavnedMobsCount = 0;
			var random = new Random();
			var mobs = new Dictionary<Point, Entity>() { {Player.Position, Player } };
			do
			{
				var x = random.Next(0,World.GetLength(0));
				var y = random.Next(0, World.GetLength(1));
				if (World[x,y].Name=="Empty" && !mobs.ContainsKey(new Point(x,y)))
				{
					var mobPrototype = NatureMobsPrototypes[random.Next(0, NatureMobsPrototypes.Count)];
					var entity = new Entity(mobPrototype.Name, mobPrototype.Action, mobPrototype.Health, new Point(x, y),
						Sides.Down,mobPrototype.BaseDamage, new Inventory());
					entity.Inventory.AddInFirstEmptySlot(AllWorldElements["RawChicken"]);
					mobs[entity.Position] = entity;
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

		public bool PickItem(Inventory inventory,Point itemPosition)
		{
			var result = false;
			if (InBounds(itemPosition) && World[itemPosition.X,itemPosition.Y].IsItem)
				if (inventory.AddInFirstEmptySlot(World[itemPosition.X, itemPosition.Y]))
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
			World[point.X, point.Y].CanPlayerMoveIn && !Mobs.ContainsKey(point);

        public string FileName(WorldElement cell)  => @"images\world\"+cell.Name+".png";

        public string FileName(Entity entity) => @"images\entities\"+entity.Name.ToString()+entity.Direction.ToString()+".png";  
        
    }
}
