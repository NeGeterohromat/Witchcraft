using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PixelRPG
{
    public class Craft
    {
        private WorldElement[,] craft;
        private int? hash;
        public Craft(WorldElement[,] craft)
        { 
            this.craft = craft;
        }

        public override bool Equals(object? obj)
        {
            var other = obj as Craft;
            if (other == null) return false;
            if (craft.GetLength(0)!= other.craft.GetLength(0) || craft.GetLength(1) != other.craft.GetLength(1))
                return false;
            for (int i = 0; i < craft.GetLength(0); i++)
                for (int j = 0; j < craft.GetLength(1); j++)
                    if (!craft[i,j].Equals(other.craft[i,j]))
                        return false;
            return true;
        }

        public override int GetHashCode()
        {
            if (hash != null)
                return (int)hash;
            unchecked
            {
                int hash = 17;
                foreach (var item in craft)
                    hash = hash * 23 + item.GetHashCode();
                return hash;
            }
        }
    }
}
