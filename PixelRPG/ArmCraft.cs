using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class ArmCraft : Inventory
    {
        public WorldElement[,] CraftZone {  get; private set; }
        public WorldElement[,] CraftResult { get; private set; }
        private InventoryTypes[,] rightSide;
        public ArmCraft()
        {
            CraftZone = new WorldElement[2, 2];
            CraftResult = new WorldElement[1, 2];
            SetEmptyArray(CraftZone);
            SetEmptyArray(CraftResult);
        }

        public bool Craft(Dictionary<Craft, WorldElement[,]> crafts)
        {
            var craft = new Craft(CraftZone);
            if (crafts.ContainsKey(craft))
            {
                CraftResult = CopyArray(crafts[craft]);
                SetEmptyArray(CraftZone);
                return true;
            }
            return false;
        }

        public override (bool IsComplete, (int X, int Y, InventoryTypes Type) First, (int X, int Y, InventoryTypes Type) Second) ChangeSelectedSlots()
        {
            if (SelectedSlots.Count == 2)
            {
                var first = SelectedSlots.Dequeue();
                var second = SelectedSlots.Dequeue();
                switch (first.Type)
                {
                    case InventoryTypes.Main:
                        SwitchSecondTypes(InventorySlots, (first.X, first.Y), second.Type, (second.X,second.Y));
                        break;
                    case InventoryTypes.Craft:
                        SwitchSecondTypes(CraftZone, (first.X, first.Y), second.Type, (second.X, second.Y));
                        break;
                    case InventoryTypes.Result:
                        SetEmptyArray(CraftZone);
                        SwitchSecondTypes(CraftResult, (first.X, first.Y), second.Type, (second.X, second.Y));
                        break;
                }
                return (true, first, second);
            }
            return (false, (-1, -1, InventoryTypes.Main), (-1, -1, InventoryTypes.Main));
        }

        public override InventoryTypes[,] GetRightSide()
        {
            if (rightSide == null)
            {
                var table = new InventoryTypes[CraftZone.GetLength(0) + 2, InventorySlots.GetLength(1)];
                for (int i = 0; i < table.GetLength(0); i++)
                    for (int j = 0; j < table.GetLength(1); j++)
                    {
                        if (i > 0 && i < table.GetLength(0) - 1 && j > 0 && j < CraftZone.GetLength(1) + 1)
                            table[i, j] = InventoryTypes.Craft;
                        else if (i > 0 && i < table.GetLength(0) - 1 && j > CraftZone.GetLength(1) + 1
                            && j < CraftZone.GetLength(1) + 1 + CraftResult.GetLength(1))
                            table[i, j] = InventoryTypes.Result;
                        else
                            table[i, j] = InventoryTypes.None;
                    }
                rightSide = table;
            }
            return rightSide;
        }

        private void SwitchSecondTypes(WorldElement[,] first, (int X, int Y) firstPlace, InventoryTypes second, (int X, int Y) secondPlace)
        {
            switch (second)
            {
                case InventoryTypes.Main:
                    ChangeSlotsInTwoArrays(first,firstPlace,InventorySlots,secondPlace);
                    break;
                case InventoryTypes.Craft:
                    ChangeSlotsInTwoArrays(first,firstPlace,CraftZone,secondPlace);
                    break;
                case InventoryTypes.Result:
                    SetEmptyArray(CraftZone);
                    ChangeSlotsInTwoArrays(first,firstPlace,CraftResult,secondPlace);
                    break;
            }
        }

        private void ChangeSlotsInTwoArrays(WorldElement[,] first,(int X, int Y)firstPlace, WorldElement[,] second, (int X, int Y) secondPlace)
        {
            var memory = first[firstPlace.X, firstPlace.Y];
            first[firstPlace.X, firstPlace.Y] = second[secondPlace.X, secondPlace.Y];
            second[secondPlace.X, secondPlace.Y] = memory;
        }
    }
}
