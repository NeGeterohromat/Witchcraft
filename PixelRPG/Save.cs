using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;

namespace PixelRPG
{
    public static class SaveManager
    {
        public static event Action<GameModel> GetModel;
        private static List<string> filePaths = new List<string>();

        public static void AddFilePath()=>filePaths.Add(@"/saves/"+filePaths.Count+@".sav");

        public static void RemoveFilePath(int i) => filePaths.RemoveAt(i);

        public static string GetFilePath(int i) => filePaths[i];
        
        public static void SaveGame(GameModel model)
        {
            var bf = new BinaryFormatter();
            AddFilePath();
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = appDir+filePaths.Last();
            var fs = new FileStream(fullPath, FileMode.Create);
            var save = new Save(model);
            bf.Serialize(fs, save);
            fs.Close();
        }

        public static GameModel LoadGame(string filePath) 
        {
            var appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var fullPath = appDir + filePaths.Last();
            if (!File.Exists(fullPath))
                throw new Exception("File path is incorrect");
            var bf = new BinaryFormatter();
            var fs = new FileStream(fullPath, FileMode.Open);
            var save = (Save)bf.Deserialize(fs);
            fs.Close();
            return GameModel.GetModel(save);
        }
    }

    [System.Serializable]
    public class Save
    {
        [System.Serializable]
        public struct World
        {
            public string[,] WorldElements;
            public World(WorldElement[,] els) 
            { 
                WorldElements = new string[els.GetLength(0),els.GetLength(1)];
                for (int i = 0; i< els.GetLength(0); i++)
                    for (int j = 0; j< els.GetLength(1); j++)
                        WorldElements[i, j] = els[i, j].Name;
            }
        }

        [System.Serializable]
        public struct ChestData
        {
            public Point Position;
            public string[,] Inventory;

            public ChestData(Point pos, WorldElement[,] els)
            {
                Position = pos;
                Inventory = new string[els.GetLength(0), els.GetLength(1)];
                for (int i = 0;i< els.GetLength(0);i++)
                    for (int j = 0;j< els.GetLength(1);j++)
                        Inventory[i,j] = els[i,j].Name;
            }
        }

        [System.Serializable]
        public struct Magic
        {
            public int[,] DamageRange;
            public int ManaWasting;
            public SpellType Type;
        }

        [System.Serializable]
        public struct EntityData
        {
            public int RegenerationExp;
            public int DamageExp;
            public int SatietyExp;
            public int ManaExp;
            public int Health;
            public Point Position;
            public Sides Direction;
            public string[,] Inventory;
            public int BaseDamage;
            public int MaxHealth;
            public int Satiety;
            public int MaxSatiety;
            public int Mana;
            public int MaxMana;
            public Magic[] Spells;
            public Magic CurrentSpell;
            public int CurrentSpellIndex;
            public string Name;
            public EntityActionType Action;

            public EntityData(Entity en)
            {
                Name = en.Name;
                Position = en.Position;
                RegenerationExp = en.RegenerationExp;
                DamageExp = en.DamageExp;
                SatietyExp = en.SatietyExp;
                ManaExp = en.ManaExp;
                Health = en.Health;
                Direction = en.Direction;
                Inventory = new string[en.Inventory.InventorySlots.GetLength(0), en.Inventory.InventorySlots.GetLength(1)];
                for (int i = 0; i < en.Inventory.InventorySlots.GetLength(0); i++)
                    for (int j = 0; j < en.Inventory.InventorySlots.GetLength(1); j++)
                        Inventory[i, j] = en.Inventory.InventorySlots[i, j].Name;
                BaseDamage = en.BaseDamage;
                MaxHealth = en.MaxHealth;
                Satiety = en.Satiety;
                MaxSatiety = en.MaxSatiety;
                Mana = en.Mana;
                MaxMana = en.MaxMana;
                Spells = en.Spells.Select(s=>new Magic() { DamageRange=s.DamageRange, ManaWasting =s.ManaWasting, Type = s.Type}).ToArray();
                CurrentSpell = new Magic() { DamageRange = en.CurrentSpell.DamageRange, ManaWasting = en.CurrentSpell.ManaWasting, Type = en.CurrentSpell.Type };
                CurrentSpellIndex = en.CurrentSpellIndex;
                Action = en.Action;
            }
        }

        public List<EntityData> Entities;
        public World WorldData;
        public List<ChestData> Chests;


        public Save(GameModel model)
        {
            WorldData = new World(model.World);
            Entities = model.Mobs.Values.Select(en => new EntityData(en)).ToList();
            Chests = model.Chests.Select(m => new ChestData(m.Key, m.Value.ChestInventory.InventorySlots)).ToList();
        }
    }
}
