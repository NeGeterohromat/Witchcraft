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
        public Queue<(int X, int Y,InventoryTypes Type)> SelectedSlots {  get; private set; }
        public Inventory()
        {
            SelectedSlots = new Queue<(int X, int Y, InventoryTypes Type)>();
            InventorySlots = new WorldElement[8, 8];
            SetEmptyArray(InventorySlots);
        }
        public void SetEmptyArray(WorldElement[,] array)
        {
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    array[i, j] = new WorldElement("Empty", false, int.MaxValue,true);
        }

        public WorldElement[,] CopyArray(WorldElement[,] array)
        {
            var newArray = new WorldElement[array.GetLength(0),array.GetLength(1)];
            for (int i = 0;i < newArray.GetLength(0);i++)
                for (int j = 0;j < newArray.GetLength(1);j++)
                    newArray[i,j] = array[i,j];
            return newArray;
        }

        public virtual InventoryTypes[,] GetRightSide()
        {
            return new InventoryTypes[1, InventorySlots.GetLength(1)];
        }

        public void ClearSlots()=> SelectedSlots.Clear();

        public bool SetItemInFirstSlot()
        {
            if (InventorySlots[0, 0].Name != "Empty") return false;
            var result = false;
            for (int i = 0; i < InventorySlots.GetLength(0); i++)
            {
                for (int j = 0; j < InventorySlots.GetLength(1); j++)
                    if (InventorySlots[i, j].Name != "Empty")
                    {
                        InventorySlots[0,0] = InventorySlots[i,j];
                        InventorySlots[i, j] = new WorldElement("Empty",false,int.MaxValue,true);
                        result = true;
                        break;
                    }
                if (result)
                    break;
            }
            return result;
        }

        public virtual (bool IsComplete, (int X, int Y, InventoryTypes Type) First, (int X, int Y, InventoryTypes Type) Second) ChangeSelectedSlots()
        {
            if (SelectedSlots.Count == 2)
            {
                var first = SelectedSlots.Dequeue();
                var second = SelectedSlots.Dequeue();
                if (first.Type == InventoryTypes.Main && second.Type == InventoryTypes.Main)
                {
                    var memory = InventorySlots[first.X, first.Y];
                    InventorySlots[first.X, first.Y] = InventorySlots[second.X, second.Y];
                    InventorySlots[second.X, second.Y] = memory;
                    return (true, first, second);
                }
            }
            return (false,(-1,-1,InventoryTypes.Main),(-1,-1, InventoryTypes.Main));
        }

        public bool AddSlot(int x, int y,InventoryTypes type)
        {
            if (SelectedSlots.Count < 2)
            {
                SelectedSlots.Enqueue((x, y,type));
                return true;
            }
            SelectedSlots.Dequeue();
            SelectedSlots.Enqueue((x, y,type));
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
