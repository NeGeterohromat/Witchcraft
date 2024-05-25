using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class SpellTableInterface : Inventory
    {
        public MagicSpell[,] SavedSpells {  get; private set; }
        public MagicSpell[,] PlayerSpells { get; private set; }
        public MagicSpell[,] Result {  get; private set; }

        public SpellTableInterface(MagicSpell[] playerSpells)
        {
            PlayerSpells = new MagicSpell[1,playerSpells.Length];
            Result = new MagicSpell[1, 1] { { new MagicSpell(new int[0, 0], 0, SpellType.Empty) } };
            for (int i = 0; i < playerSpells.Length; i++)
                PlayerSpells[0,i] = playerSpells[i];
            SavedSpells = new MagicSpell[5, 5];
            SetEmptyArray(SavedSpells);
        }

        public static void SetEmptyArray(MagicSpell[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    array[i, j] = new MagicSpell(new int[0,0],0,SpellType.Empty);
        }

        public MagicSpell Craft(string text) => Result[0,0]=MagicSpell.TranslateSpellWords(text);

        public override (bool IsComplete, (int X, int Y, InventoryTypes Type) First, (int X, int Y, InventoryTypes Type) Second) ChangeSelectedSlots()
        {
            if (SelectedSlots.Count == 2)
            {
                var first = SelectedSlots.Dequeue();
                var second = SelectedSlots.Dequeue();
                switch (first.Type)
                {
                    case InventoryTypes.SpellInventory:
                        SwitchSecondTypes(SavedSpells, (first.X, first.Y), second.Type, (second.X, second.Y));
                        break;
                    case InventoryTypes.SpellSlots:
                        SwitchSecondTypes(PlayerSpells, (first.X, first.Y), second.Type, (second.X, second.Y));
                        break;
                    case InventoryTypes.Result:
                        SwitchSecondTypes(Result, (first.X, first.Y), second.Type, (second.X, second.Y));
                        break;
                }
                return (true, first, second);
            }
            return (false, (-1, -1, InventoryTypes.Main), (-1, -1, InventoryTypes.Main));
        }

        private void SwitchSecondTypes(MagicSpell[,] first, (int X, int Y) firstPlace, InventoryTypes second, (int X, int Y) secondPlace)
        {
            switch (second)
            {
                case InventoryTypes.SpellInventory:
                    ChangeSlotsInTwoArrays(first, firstPlace, SavedSpells, secondPlace);
                    break;
                case InventoryTypes.SpellSlots:
                    ChangeSlotsInTwoArrays(first, firstPlace, PlayerSpells, secondPlace);
                    break;
                case InventoryTypes.Result:
                    ChangeSlotsInTwoArrays(first, firstPlace, Result, secondPlace);
                    break;
            }
        }

        private void ChangeSlotsInTwoArrays(MagicSpell[,] first, (int X, int Y) firstPlace, MagicSpell[,] second, (int X, int Y) secondPlace)
        {
            var memory = first[firstPlace.X, firstPlace.Y];
            first[firstPlace.X, firstPlace.Y] = second[secondPlace.X, secondPlace.Y];
            second[secondPlace.X, secondPlace.Y] = memory;
        }
    }
}
