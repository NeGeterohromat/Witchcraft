using System;

namespace PixelRPG
{
	public class GameModel
	{
		public Entity Player { get; private set; }
		public WorldElement[,] World { get; private set; }
		public Dictionary<Point,Entity> Mobs { get; private set; }
        public Dictionary<Point, Chest> Chests { get; private set; }
        public readonly WorldElement OutOfBounds = new WorldElement(WorldElementType.Struckture,"OutOfBounds");
		private const double emptyPercent = 3d / 4;
		private int MobCount;
		private bool isEnemySpawn = true;
        public const double peacefulMobMoveChance = 1d / 10;
        public const int WorldTick = 1*1000;
		public const double enemyMobMoveChance = 1d / 2;
		public const double DecreacingSatietyChance = 1d / 20;
		public const double ChangeHealthBecauseOfFoodChance = 1d / 3;
		public const double IncreacingManaChance = 1d / 3;
        public readonly List<WorldElement> NatureWorldElementsList;
		public readonly Dictionary<Craft, WorldElement[,]> Crafts2by2;
		public SpellTableInterface SpellTableSaved {  get; private set; }
		public readonly List<Entity> NatureMobsPrototypes;
		public readonly Dictionary<string, Entity> AllMobsPrototypes = new Dictionary<string, Entity>()
		{
			{"Player",new Entity("Player",EntityActionType.Player,20,new Point(-1,-1),Sides.Down,1,20,20,new ArmCraft()) },
			{"Chicken",new Entity("Chicken",EntityActionType.Peaceful,10,new Point(-1,-1),Sides.Down,0,20,0,new Inventory()) },
			{"Zombie", new Entity("Zombie",EntityActionType.Enemy,20,new Point(-1,-1),Sides.Down,2,20,20, new Inventory()) }
		};
		public readonly Dictionary<string, List<(WorldElement El, double Chance)>> EntityDrops = new Dictionary<string, List<(WorldElement El, double Chance)>>();
		public static readonly Dictionary<string, WorldElement> AllWorldElements = new Dictionary<string, WorldElement>()
		{
			{"OutOfBounds",new WorldElement(WorldElementType.Struckture,"OutOfBounds") },
			{"Empty",new WorldElement(WorldElementType.Empty,"Empty",int.MaxValue,true) },
			{"Grass", new WorldElement(WorldElementType.Struckture,"Grass",0,true,new WorldElement(WorldElementType.Thing,"Turf"))},
			{"Turf",new WorldElement(WorldElementType.Thing, "Turf") },
			{"Tree",  new WorldElement(WorldElementType.Struckture,"Tree",1,false,new WorldElement(WorldElementType.Thing,"Wood"))},
			{"Wood", new WorldElement(WorldElementType.Thing, "Wood")},
			{"Stone",  new WorldElement(WorldElementType.Thing, "Stone")},
			{"Bush",new WorldElement(WorldElementType.Struckture,"Bush",0,false, new WorldElement(WorldElementType.Thing, "Stick")) },
			{"Stick", new WorldElement(WorldElementType.Thing, "Stick")},
			{"StoneChopper",new WorldElement(WorldElementType.Thing, "StoneChopper",int.MaxValue,true,1,5) },
			{"Heap",new WorldElement(WorldElementType.Struckture,"Heap",int.MaxValue,false,null,true) },
			{"RawChicken",new WorldElement(WorldElementType.Food, "RawChicken",5) },
			{"WoodenPlanks",new WorldElement(WorldElementType.Block,"WoodenPlanks",1,false,new WorldElement(WorldElementType.Thing,"WoodenPlanksItem","WoodenPlanks")) },
			{"DirtBlock",new WorldElement(WorldElementType.Block,"DirtBlock",0,false,new WorldElement(WorldElementType.Thing,"DirtBlockItem","DirtBlock")) },
			{"WoodenPlanksItem", new WorldElement(WorldElementType.Thing,"WoodenPlanksItem","WoodenPlanks")},
			{"DirtBlockItem", new WorldElement(WorldElementType.Thing,"DirtBlockItem","DirtBlock")},
			{"RottenFlesh", new WorldElement(WorldElementType.Food,"RottenFlesh",1) },
			{"ZombieHeart", new WorldElement(WorldElementType.Thing,"ZombieHeart") },
			{"MagicDust", new WorldElement(WorldElementType.Thing,"MagicDust") },
			{"SpellTable", new WorldElement(WorldElementType.Block,"SpellTable",0,false,new WorldElement(WorldElementType.Thing,"SpellTableItem","SpellTable")) },
			{"SpellTableItem", new WorldElement(WorldElementType.Thing,"SpellTableItem","SpellTable")},
			{"WoodenChest", new WorldElement(WorldElementType.Block,"WoodenChest",1,false,new WorldElement(WorldElementType.Thing,"WoodenChestItem","WoodenChest"),true) },
			{"WoodenChestItem", new WorldElement(WorldElementType.Thing,"WoodenChestItem","WoodenChest",true) }
        };

		public GameModel() { }

        public GameModel(int worldSize, bool isEnemySpawn)
		{
            this.isEnemySpawn = isEnemySpawn;
			MobCount = worldSize * worldSize * 25 / 40 / 40;
            Crafts2by2 = GetCrafts();
            NatureWorldElementsList = GetNatureWorldElementList();
            EntityDrops = GetEntityDrops();
            Player = AllMobsPrototypes["Player"];
			Player.IncreaseHealth(Player.MaxHealth);
			Player.IncreaseSatiety(Player.MaxSatiety);
			Player.AddFirstSpell(new MagicSpell(new int[3, 3]
            {
                {0,1,0 },
                {1,2,1 },
                {0,1,0 }
            }, 3, SpellType.Damage));
            World = CreateWorld(worldSize);
			Player.SetPosition(FindFirstSpawnPoint());
			NatureMobsPrototypes = GetNatureMobs();
			Mobs = GetWorldMobs();
			Chests = new Dictionary<Point, Chest>();
			SpellTableSaved = new SpellTableInterface(Player.Spells);
		}
		public Point FindFirstSpawnPoint()
		{
			for (int i = 1; i < World.GetLength(0); i++)
				for (int j = 1; j < World.GetLength(1); j++)
					if (World[i, j].Type == WorldElementType.Empty)
						return new Point(i, j);
			return new Point(0,0);
		}

		public Dictionary<string, List<(WorldElement El, double Chance)>> GetEntityDrops()
		{
			return new Dictionary<string, List<(WorldElement El, double Chance)>>()
			{
				{"Chicken", new List<(WorldElement El, double Chance)>(){(AllWorldElements["RawChicken"],1) } },
				{"Zombie", new List<(WorldElement El, double Chance)>(){(AllWorldElements["RottenFlesh"],1),(AllWorldElements["ZombieHeart"],2d/3) } }
			};
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
				if (World[x,y].Type == WorldElementType.Empty && !mobs.ContainsKey(new Point(x,y)))
				{
					var mobPrototype = NatureMobsPrototypes[random.Next(0, NatureMobsPrototypes.Count)];
					if (isEnemySpawn || (!isEnemySpawn && mobPrototype.Action != EntityActionType.Enemy))
					{
						var entity = new Entity(mobPrototype.Name, mobPrototype.Action, mobPrototype.Health, new Point(x, y),
						Sides.Down, mobPrototype.BaseDamage, mobPrototype.Satiety, mobPrototype.Mana, new Inventory());
						if (EntityDrops.ContainsKey(entity.Name))
							foreach (var item in EntityDrops[entity.Name])
								if (random.NextDouble() < item.Chance)
									entity.Inventory.AddInFirstEmptySlot(item.El);
						entity.AddFirstSpell(new MagicSpell(new int[0, 0], 0, SpellType.Empty));
						mobs[entity.Position] = entity;
						spavnedMobsCount++;
					}
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
				},
				{
					new Craft(new WorldElement[2,2]
					{
                        { AllWorldElements["Wood"], AllWorldElements["StoneChopper"] },
                        { AllWorldElements["Empty"],AllWorldElements["Empty"] }
                    }),
                    new WorldElement[1,2]
                    {
                        {AllWorldElements["WoodenPlanksItem"], AllWorldElements["StoneChopper"] }
                    }
                },
                {
                    new Craft( new WorldElement[2,2]
                    {
                        { AllWorldElements["Turf"], AllWorldElements["Turf"] },
                        { AllWorldElements["Turf"],AllWorldElements["Turf"] }
                    }),
                    new WorldElement[1,2]
                    {
                        {AllWorldElements["DirtBlockItem"], AllWorldElements["Empty"] }
                    }
                },
                {
                    new Craft(new WorldElement[2,2]
                    {
                        { AllWorldElements["ZombieHeart"], AllWorldElements["ZombieHeart"] },
                        { AllWorldElements["Empty"],AllWorldElements["Empty"] }
                    }),
                    new WorldElement[1,2]
                    {
                        {AllWorldElements["MagicDust"], AllWorldElements["Empty"] }
                    }
                },
                {
                    new Craft(new WorldElement[2,2]
                    {
                        { AllWorldElements["MagicDust"], AllWorldElements["MagicDust"] },
                        { AllWorldElements["MagicDust"],AllWorldElements["Wood"] }
                    }),
                    new WorldElement[1,2]
                    {
                        {AllWorldElements["SpellTableItem"], AllWorldElements["Empty"] }
                    }
                },
                {
                    new Craft(new WorldElement[2,2]
                    {
                        { AllWorldElements["WoodenPlanksItem"], AllWorldElements["StoneChopper"] },
                        { AllWorldElements["Empty"],AllWorldElements["Empty"] }
                    }),
                    new WorldElement[1,2]
                    {
                        {AllWorldElements["WoodenChestItem"], AllWorldElements["StoneChopper"] }
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

        public string FileName(MagicSpell spell) => @"images\icons\" + spell.ImageType + ".png";

		public void ConnectChestsWithPlayer() => Chests = Chests.ToDictionary(ch=>ch.Key,ch=>new Chest(ch.Value.ChestInventory,Player.Inventory));
		
		public static GameModel GetModel(Save save)
		{
			var game = new GameModel()
			{
				Mobs = save.Entities.Select(en => Entity.GetEntity(en)).ToDictionary(en => en.Position, en => en),
				World = GetWorld(save.WorldData.WorldElements),
				Chests = save.Chests.ToDictionary(ch => ch.Position, ch => new Chest(Inventory.GetInventory(ch.Inventory), new ArmCraft()))
			};
			game.ConnectChestsWithPlayer();
			game.Player = game.Mobs.Values.Where(en => en.Action==EntityActionType.Player).First();
			return game;
		}

		public static WorldElement[,] GetWorld(string[,] world)
		{
			var result = new WorldElement[world.GetLength(0),world.GetLength(1)];
			for (int i = 0;  i < world.GetLength(0); i++)
				for (int j = 0; j < world.GetLength(1); j++)
					result[i,j] = AllWorldElements[world[i,j]];
			return result;
		}
    }
}
