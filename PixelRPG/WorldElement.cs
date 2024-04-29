using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class WorldElement
    {
        public readonly string Name;
        public readonly bool CanPlayerMoveIn;
        public readonly bool IsItem;
        public readonly int BreakLevel;
        public readonly int PowerToBreakOtherEl;
        public readonly WorldElement Drop;
        private int? hash;
        public WorldElement(string name, bool isItem, int breakLevel ,
            bool? canPlayerMoveIn = null, int powerToBreakOtherEl = 0, WorldElement drop = null)
        {
            Name = name;
            CanPlayerMoveIn = (canPlayerMoveIn == null)? isItem:(bool)canPlayerMoveIn;
            IsItem = isItem;
            BreakLevel = breakLevel;
            Drop = drop;
            PowerToBreakOtherEl = powerToBreakOtherEl;
        }

        public override bool Equals(object? obj)
        {
            var el = obj as WorldElement;
            if (el == null) return false;
            return el.Name == Name && (el.CanPlayerMoveIn == CanPlayerMoveIn) && (el.IsItem == IsItem) 
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
                hash = hash * 23 + IsItem.GetHashCode();
                hash = hash * 23 + BreakLevel.GetHashCode();
                hash = hash * 23 + CanPlayerMoveIn.GetHashCode();
                hash = hash * 23 + PowerToBreakOtherEl.GetHashCode();
                this.hash = hash;
                return hash;
            }
        }
    }
}
