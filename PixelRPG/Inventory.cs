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
        public Inventory()
        {
            InventorySlots = new WorldElement[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    InventorySlots[i, j] = new WorldElement("Empty", true, false, int.MaxValue);
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
