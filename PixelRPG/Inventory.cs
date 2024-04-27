using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class Inventory
    {
        public WorldElement[,] InventorySlots {  get; private set; }
        private Queue<(int X, int Y)> selectedSlots = new Queue<(int X, int Y)>();
        public Inventory()
        {
            InventorySlots = new WorldElement[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    InventorySlots[i, j] = new WorldElement("Empty", true, false, int.MaxValue);
        }

        public void ClearSlots()=> selectedSlots.Clear();

        public (bool IsComplete, (int X, int Y) First, (int X, int Y) Second) ChangeSelectedSlots()
        {
            if (selectedSlots.Count == 2)
            {
                var first = selectedSlots.Dequeue();
                var second = selectedSlots.Dequeue();
                var memory = InventorySlots[first.X, first.Y];
                InventorySlots[first.X, first.Y] = InventorySlots[second.X, second.Y];
                InventorySlots[second.X,second.Y] = memory;
                return (true,first,second);
            }
            return (false,(-1,-1),(-1,-1));
        }

        public bool AddSlot(int x, int y)
        {
            if (selectedSlots.Count < 2)
            {
                selectedSlots.Enqueue((x, y));
                return true;
            }
            selectedSlots.Dequeue();
            selectedSlots.Enqueue((x, y));
            return false;
            
        }

        public bool AddInFirstEmptySlot(WorldElement item)
        {
            var result = false;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                    if (InventorySlots[i, j].Name == "Empty")
                    {
                        InventorySlots[i, j] = item;
                        result = true;
                        break;
                    }
                if (result)
                    break;
            }
            return result;
        }
    }
}
