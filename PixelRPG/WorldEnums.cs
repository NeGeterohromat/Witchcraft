using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public enum Sides
    {
        Left,
        Right,
        Up,
        Down
    }
    public enum InventoryTypes
    {
        None,
        Main,
        Craft,
        Result
    }
    public enum EntityActionType
    {
        Player,
        Peaceful,
        Enemy
    }
}
