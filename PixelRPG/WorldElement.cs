using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PixelRPG
{
    public class WorldElement
    {
        public readonly string Name;
        public readonly bool CanPlayerMoveIn;
        public readonly int BreakLevel;
        public readonly int PowerToBreakOtherEl;
        public readonly WorldElement Drop;
        public readonly string? ParentBlockName;
        public readonly int Damage;
        public readonly bool IsItem;
        public readonly WorldElementType Type;
        public int? SatietyBonus;
        private int? hash;
        public WorldElement(WorldElementType type,string name, int breakLevel,
            bool? canPlayerMoveIn = null, int powerToBreakOtherEl = 0,
             int damage=1)
        {
            IsItem = (type == WorldElementType.Thing) || (type == WorldElementType.Food) || (type == WorldElementType.Armor);
            Name = name;
            CanPlayerMoveIn = (canPlayerMoveIn == null)? IsItem :(bool)canPlayerMoveIn;
            BreakLevel = breakLevel;
            Drop = null;
            PowerToBreakOtherEl = powerToBreakOtherEl;
            Damage = damage;
            Type = type;
            SatietyBonus = null;
            ParentBlockName = null;
        }

        public WorldElement(WorldElementType type,string name, int breakLevel, bool canPlayerMoveIn, WorldElement drop)
        {
            IsItem = (type == WorldElementType.Thing) || (type == WorldElementType.Food) || (type == WorldElementType.Armor);
            Name = name;
            CanPlayerMoveIn = canPlayerMoveIn;
            BreakLevel = breakLevel;
            Drop = drop;
            PowerToBreakOtherEl = 0;
            Damage = 1;
            Type = type;
            SatietyBonus = null;
            ParentBlockName = null;
        }

        public WorldElement(WorldElementType type, string name, string? parentBlockName = null)
        {
            IsItem = (type == WorldElementType.Thing) || (type == WorldElementType.Food) || (type == WorldElementType.Armor);
            Name = name;
            CanPlayerMoveIn = true;
            BreakLevel = int.MaxValue;
            Drop = null;
            PowerToBreakOtherEl = 0;
            Damage = 1;
            Type = type;
            SatietyBonus = null;
            ParentBlockName = parentBlockName;
        }

        public WorldElement(WorldElementType type,string name, int? satietyBonus)
        {
            IsItem = (type == WorldElementType.Thing) || (type == WorldElementType.Food) || (type == WorldElementType.Armor);
            Name = name;
            CanPlayerMoveIn = true;
            BreakLevel = int.MaxValue;
            Drop = null;
            PowerToBreakOtherEl = 0;
            Damage = 1;
            Type = type;
            SatietyBonus = satietyBonus;
            ParentBlockName = null;
        }

        public override bool Equals(object? obj)
        {
            var el = obj as WorldElement;
            if (el == null) return false;
            return el.Name == Name && (el.CanPlayerMoveIn == CanPlayerMoveIn)
                && el.BreakLevel == BreakLevel && el.PowerToBreakOtherEl == PowerToBreakOtherEl;
        }

        public override int GetHashCode()
        {
            if (hash != null)
                return (int)hash;
            unchecked 
            {
                int hash = 17;
                hash = hash * 23 + Name.GetHashCode();
                hash = hash * 23 + BreakLevel.GetHashCode();
                hash = hash * 23 + CanPlayerMoveIn.GetHashCode();
                hash = hash * 23 + PowerToBreakOtherEl.GetHashCode();
                this.hash = hash;
                return hash;
            }
        }
    }
}
