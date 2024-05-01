using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelRPG
{
    public class Mob
    {
        public readonly string Name;
        public readonly MobActionType Action;
        public int Health {  get; private set; }
        public Point Position { get; private set; }
        public Mob(string name, MobActionType action, int health, Point position)
        {
            Name = name;
            Action = action;
            Health = health;
            Position = position;
        }
    }
}
