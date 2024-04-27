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
        public readonly WorldElement Drop;
        public WorldElement(string name, bool canPlayerMoveIn, bool isItem, int breakLevel, WorldElement drop = null)
        {
            Name = name;
            CanPlayerMoveIn = canPlayerMoveIn;
            IsItem = isItem;
            BreakLevel = breakLevel;
            Drop = drop;
        }
    }
}
