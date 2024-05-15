using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class Entity
    {
        public readonly string Name;
        public readonly EntityActionType Action;
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
        public List<MagicSpell> Spells { get; private set; }
        public MagicSpell CurrentSpell { get; private set; }
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
            Spells = new List<MagicSpell>();
        }

        public void AddSpell(MagicSpell spell)
        {
            Spells.Add(spell);
            if (CurrentSpell == null)
                CurrentSpell = spell;
        }

        public void IncreaseMana(int mana)
        {
            if (mana >= 0)
            {
                if (mana + Mana <= MaxMana)
                    Mana += mana;
                else
                    Mana = MaxMana;
            }
            else
                throw new Exception("Increased mana amount must be non negative!");
        }

        public bool DecreaseMana(int mana)
        {
            if (mana >= 0)
            {
                if (Mana - mana >= 0)
                {
                    Mana -= mana;
                    return true;
                }
                else
                {
                    Mana = 0;
                    return false;
                }
            }
            else
                throw new Exception("Decreased mana amount must be non negative!");
        }

        public void IncreaseSatiety(int sat)
        {
            if (sat >= 0)
            {
                if (sat + Satiety <= MaxHealth)
                    Satiety += sat;
                else
                    Satiety = MaxSatiety;
            }
            else
                throw new Exception("Increased mana amount must be non negative!");
        }

        public void DecreaseSatiety(int sat)
        {
            if (sat >= 0)
            {
                if (Satiety - sat >= 0)
                    Satiety -= sat;
                else
                    Satiety = 0;
            }
            else
                throw new Exception("Decreased mana amount must be non negative!");
        }

        public void ChangeMaxSatiety(int sat)
        {
            if (sat >= 0)
                MaxSatiety = sat;
            else throw new Exception("MaxSatiety must be non negative!");
        }

        public void IncreaseHealth(int health)
        {
            if (health >= 0)
            {
                if (health + Health <= MaxHealth)
                    Health += health;
                else
                    Health = MaxHealth;
            }
            else
                throw new Exception("Increased health amount must be non negative!");
        }

        public void ChangeMaxHealth(int health)
        {
            if (health >= 0)
                MaxHealth = health;
            else throw new Exception("MaxHealth must be non negative!");
        }

        public void SetPosition(Point newPosition) => Position = newPosition;
        public void SetDirection(Sides dir) => Direction = dir;
        public void DamageEntity(int damage) 
        {
            var newHealth = Health - damage;
            if (newHealth > 0)
                Health = newHealth;
            else
                Health = 0;
        }
    }
}
