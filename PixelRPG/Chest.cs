using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{ 
    public class Chest : Inventory
    {
        public Inventory ChestInventory { get; private set; }
        private InventoryTypes[,] rightSide;
        public Chest(Inventory chestInventory, Inventory playerInventory)
        {
            ChestInventory = chestInventory;
            InventorySlots = playerInventory.InventorySlots;
        }

        public bool IsEmpty()
        {
            foreach (var slot in ChestInventory.InventorySlots) 
                if (slot.Name != "Empty")
                    return false;
            return true;
        }

        public override InventoryTypes[,] GetRightSide()
        {
            if (rightSide == null)
            {
                var table = new InventoryTypes[ChestInventory.InventorySlots.GetLength(0) + 1, InventorySlots.GetLength(1)];
                for (int i = 0; i < table.GetLength(0); i++)
                    for (int j = 0; j < table.GetLength(1); j++)
                    {
                        if (i == 0)
                            table[i, j] = InventoryTypes.None;
                        else
                            table[i, j] = InventoryTypes.Chest;
                    }
                rightSide = table;
            }
            return rightSide;
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
                        SwitchSecondTypes(InventorySlots, (first.X, first.Y), second.Type, (second.X, second.Y));
                        break;
                    case InventoryTypes.Chest:
                        SwitchSecondTypes(ChestInventory.InventorySlots, (first.X, first.Y), second.Type, (second.X, second.Y));
                        break;
                }
                return (true, first, second);
            }
            return (false, (-1, -1, InventoryTypes.Main), (-1, -1, InventoryTypes.Main));
        }

        private void SwitchSecondTypes(WorldElement[,] first, (int X, int Y) firstPlace, InventoryTypes second, (int X, int Y) secondPlace)
        {
            switch (second)
            {
                case InventoryTypes.Main:
                    ChangeSlotsInTwoArrays(first, firstPlace, InventorySlots, secondPlace);
                    break;
                case InventoryTypes.Chest:
                    ChangeSlotsInTwoArrays(first, firstPlace, ChestInventory.InventorySlots, secondPlace);
                    break;
            }
        }

        private void ChangeSlotsInTwoArrays(WorldElement[,] first, (int X, int Y) firstPlace, WorldElement[,] second, (int X, int Y) secondPlace)
        {
            var memory = first[firstPlace.X, firstPlace.Y];
            first[firstPlace.X, firstPlace.Y] = second[secondPlace.X, secondPlace.Y];
            second[secondPlace.X, secondPlace.Y] = memory;
        }
    }
}
