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
        public WorldElement(string name, bool canPlayerMoveIn)
        {
            Name = name;
            CanPlayerMoveIn = canPlayerMoveIn;
        }
    }
}
