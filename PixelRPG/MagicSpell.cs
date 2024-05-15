using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class MagicSpell
    {
        public readonly int[,] DamageRange;
        public readonly int ManaWasting;
        public MagicSpell(int[,] damageRange, int manaWasting) 
        { 
            DamageRange = damageRange;
            ManaWasting = manaWasting;
        }
    }
}
