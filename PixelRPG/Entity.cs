using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class Entity
    {
        public readonly string Name;
        public readonly EntityActionType Action;
        public int Health {  get; private set; }
        public Point Position { get; private set; }
        public Sides Direction { get; private set; }
        public Inventory Inventory { get; private set; }
        public Entity(string name, EntityActionType action, int health, Point position, Sides direction, Inventory inventory)
        {
            Name = name;
            Action = action;
            Health = health;
            Position = position;
            Direction = direction;
            Inventory = inventory;
        }

        public void SetPosition(Point newPosition) => Position = newPosition;
        public void SetDirection(Sides dir) => Direction = dir;
    }
}
