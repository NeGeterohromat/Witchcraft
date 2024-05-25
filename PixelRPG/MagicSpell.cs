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
        public readonly SpellType Type;
        public readonly string ImageType;
        private static readonly Dictionary<string, SpellType> spellTypes = new Dictionary<string, SpellType>()
        {
            {"lfvf;m",SpellType.Damage }
        };
        private static readonly Dictionary<string, int> numbers = new Dictionary<string, int>()
        {
            {"yjkm",0 },
            {"jlby",1 },
            {"ldf",2 },
            {"nhb",3 },
            {"xtnsht",4 },
            {"gznm",5 },
            {"itcnm",6 },
            {"ctvm",7 }
        };
        public MagicSpell(int[,] damageRange, int manaWasting,SpellType type) 
        { 
            DamageRange = damageRange;
            ManaWasting = manaWasting;
            Type = type;
            ImageType = type.ToString()+(manaWasting>11?3:manaWasting/4).ToString();
        }

        public static MagicSpell TranslateSpellWords(string text)
        {
            var words = text.Split(' ');
            SpellType type = default;
            int[,] damageRange = default;
            if (words.Length < 2)
                return new MagicSpell(new int[0, 0], 0, SpellType.Empty);
            if (spellTypes.ContainsKey(words[0]))
                type = spellTypes[words[0]];
            else
                return new MagicSpell(new int[0, 0], 0, SpellType.Empty);
            if (type == SpellType.Damage)
            {
                int range = default;
                var damageSum = 0;
                if (numbers.ContainsKey(words[1]))
                    range = numbers[words[1]];
                else
                    return new MagicSpell(new int[0, 0], 0, SpellType.Empty);
                if (range % 2 == 1)
                {
                    range = range / 2+1;
                    damageRange = new int[range, range];
                }
                else
                    return new MagicSpell(new int[0, 0], 0, SpellType.Empty);
                var trio = (-1,-1,-1);
                for (int i = 2; i<words.Length;i+=3)
                {
                    if (i+2 < words.Length && numbers.ContainsKey(words[i]) && numbers.ContainsKey(words[i+1]) && numbers.ContainsKey(words[i+2]))
                    {
                        trio = (numbers[words[i]], numbers[words[i+1]], numbers[words[i+2]]);
                        if (IsTangens(trio.Item1, trio.Item2) && trio.Item1 * trio.Item3 < range && trio.Item2 * trio.Item3 < range)
                        {
                            damageRange[trio.Item1 * trio.Item3, trio.Item2 * trio.Item3] += 1;
                            damageSum += 1;
                        }
                        else
                            return new MagicSpell(new int[0, 0], 0, SpellType.Empty);
                    }
                    else
                        return new MagicSpell(new int[0, 0], 0, SpellType.Empty);
                }
                damageRange = Mirroring(damageRange);
                return new MagicSpell(damageRange, damageSum / 3 + 1, SpellType.Damage);
            }
            return new MagicSpell(new int[0, 0], 0, SpellType.Empty);
        }

        public static int[,] Mirroring(int[,] array)
        {
            var length = array.GetLength(0);
            var second = new int[length,length];
            var third = new int[length,length];
            var fourth = new int[length,length];
            for (int i = 0; i< length; i++)
                for (int j = 0; j< length; j++)
                {
                    second[length-1-i,j] = array[i,j];
                    third[length-1 - i, length-1 - j] = array[i,j];
                    fourth[i,length - 1 -j] = array[i,j];
                }
            var result = new int[length*2-1,length*2-1];
            for (int i = 0;i< length*2-1;i++)
                for (int j = 0;j< length*2-1;j++)
                    if (i<length)
                    {
                        if (j<length)
                            result[i, j] = third[i,j];
                        else
                            result[i,j] = second[i,j-length+1];
                    }
                    else
                    {
                        if (j<length)
                            result[i,j]= fourth[i-length+1,j];
                        else
                            result[i, j] = array[i-length+1,j-length+1];
                    }
            return result;
        }

        public static bool IsTangens(int first, int second)
        {
            var min = Math.Min(first, second);
            if (min != 0 && Math.Max(first, second) % min == 0)
                return false;
            for (int i = 2; i<(int)(Math.Sqrt(min)+1);i++)
                if (first % i == 0 && second % i == 0)
                    return false;
            return true;
        }
    }
}
