using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class Player
    {
        public readonly Characters Type;
        public Sides Direction {  get; private set; }
        public Point Position { get; private set; }
        public Inventory Inventory { get; private set; }
        public Player(Characters type, Point position, Sides direction)
        {
            Type = type;
            Direction = direction;
            Position = position;
            Inventory = new Inventory();
        }

        public void SetPosition(Point newPosition) => Position = newPosition;
        public void SetDirection(Sides dir) => Direction = dir;
    }
}
