using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PixelRPG.Save;

namespace PixelRPG
{
    public class Entity
    {
        public const int RegenerationExpBonus = 20 * 100;
        public const int DamageExpBonus = 50;
        public const int SatietyExpBonus = 15 * 100;
        public const int ManaExpBonus = 50;
        public readonly string Name;
        public readonly EntityActionType Action;

        public int RegenerationExp {  get; private set; }
        public int DamageExp { get; private set; }
        public int SatietyExp { get; private set; }
        public int ManaExp { get; private set; }

        public int Health {  get; private set; }
        public Point Position { get; private set; }
        public Sides Direction { get; private set; }
        public Inventory Inventory { get; private set; }
        public int BaseDamage { get; private set; }
        public int MaxHealth { get; private set; }
        public int Satiety { get; private set; }
        public int MaxSatiety { get; private set; }
        public int Mana { get; private set; }
        public int MaxMana { get; private set; }
        public MagicSpell[] Spells { get; private set; }
        public MagicSpell CurrentSpell { get; private set; }
        public int CurrentSpellIndex { get; private set; }

        public Entity(string name,EntityActionType action) 
        {
            Name = name;
            Action = action;
        }
        public Entity(string name, EntityActionType action, int health, Point position, 
            Sides direction,int baseDamage,int satiety, 
            int mana, Inventory inventory)
        {
            Name = name;
            Action = action;
            Health = health;
            Position = position;
            Direction = direction;
            Inventory = inventory;
            BaseDamage = baseDamage;
            MaxHealth = health;
            Satiety = satiety;
            MaxSatiety = satiety;
            Mana = mana;
            MaxMana = mana;
            Spells = new MagicSpell[9];
            for (int i = 0; i < Spells.Length; i++)
                Spells[i] = new MagicSpell(new int[0, 0], 0, SpellType.Empty);
            RegenerationExp = 100;
            DamageExp = 100;
            SatietyExp = 100;
            ManaExp = 100;
        }

        public void ChangeCurrentSpellIndex(int index)
        {
            if (index>=0 && index<Spells.Length)
            {
                CurrentSpellIndex = index;
                CurrentSpell = Spells[index];
            }
            else
                throw new ArgumentOutOfRangeException();
        }

        public void AddFirstSpell(MagicSpell spell)
        {
            Spells[0] = spell;
            if (CurrentSpell == null)
                ChangeCurrentSpellIndex(0);
        }

        public void IncreaseMana(int mana) => Mana = Add(mana,Mana,MaxMana);

        public bool DecreaseMana(int mana)
        {
            if (Mana >= mana)
            {
                Mana = Substract(mana, Mana);
                return true;
            }
            Mana = 0;
            return false;
        }

        public void IncreaseSatiety(int sat) => Satiety = Add(sat,Satiety,MaxSatiety);

        public void DecreaseSatiety(int sat) => Satiety = Substract(sat,Satiety);

        public void ChangeMaxSatiety(int sat)
        {
            if (sat >= 0)
                MaxSatiety = sat;
            else throw new Exception("MaxSatiety must be non negative!");
        }

        public void IncreaseHealth(int health) => Health = Add(health, Health, MaxHealth);

        public static int Add(int newValue, int currentValue, int maxValue)
        {
            if (newValue>=0)
            {
                if (newValue + currentValue <= maxValue)
                    return newValue + currentValue;
                else
                    return maxValue;
            }
            else
                throw new Exception("Value must be non negative!");
        }

        public static int Substract(int newValue, int currentValue)
        {
            if (newValue >= 0)
            {
                if (currentValue - newValue >= 0)
                    return currentValue - newValue;
                else
                    return 0;
            }
            else
                throw new Exception("Value must be non negative!");
        }

        public static int AddExperience(int newValue, int currentValue)
        {
            if (newValue >= 0)
                return currentValue + (int)(1d * newValue / currentValue);
            else
                throw new Exception("Value must be non negative!");
        }

        public void ChangeMaxHealth(int health)
        {
            if (health >= 0)
                MaxHealth = health;
            else throw new Exception("MaxHealth must be non negative!");
        }

        public void IncreaseDamageExp(int damage) => DamageExp = AddExperience(damage*DamageExpBonus,DamageExp);

        public void IncreaseSatietyExp(int sat) => SatietyExp = AddExperience(sat*SatietyExpBonus, SatietyExp);

        public void IncreaseManaExp(int mana) => ManaExp = AddExperience(mana*ManaExpBonus, ManaExp);

        public void SetPosition(Point newPosition) => Position = newPosition;
        public void SetDirection(Sides dir) => Direction = dir;
        public void DamageEntity(int damage)
        {
            Health = Substract(damage, Health);
            RegenerationExp = AddExperience(damage * RegenerationExpBonus, RegenerationExp);
        }

        public static Entity GetEntity(EntityData en)
        {
            return new Entity(en.Name, en.Action)
            {
                Position = en.Position,
                RegenerationExp = en.RegenerationExp,
                DamageExp = en.DamageExp,
                SatietyExp = en.SatietyExp,
                ManaExp = en.ManaExp,
                Health = en.Health,
                Direction = en.Direction,
                Inventory = Inventory.GetInventory(en.Inventory),
                BaseDamage = en.BaseDamage,
                MaxHealth = en.MaxHealth,
                Satiety = en.Satiety,
                MaxSatiety = en.MaxSatiety,
                Mana = en.Mana,
                MaxMana = en.MaxMana,
                Spells = en.Spells.Select(s => GetMagicSpell(s)).ToArray(),
                CurrentSpell = GetMagicSpell(en.CurrentSpell),
                CurrentSpellIndex = en.CurrentSpellIndex,
            };
        }

        public static MagicSpell GetMagicSpell(Magic magic)
        {
            if (magic.FindingElement != null)
                return new MagicSpell(magic.ManaWasting, magic.Type, GameModel.AllWorldElements[magic.FindingElement]);
            return new MagicSpell(magic.DamageRange,magic.ManaWasting,magic.Type);
        }
    }
}
