using System;

namespace Tanki
{
    enum Direction { Up, Down, Left, Right }

    abstract class Tank(int x, int y, ConsoleColor color)
    {
        public int X { get; protected set; } = x;
        public int Y { get; protected set; } = y;
        public ConsoleColor Color { get; protected set; } = color;
        public int Health { get; protected set; } = 1;
        public Direction Dir { get; protected set; } = Direction.Up;
        public int MoveCooldown { get; protected set; } = 200;
        protected int lastMoveTime = 0;

        public bool CanMoveNow(int time) => time - lastMoveTime >= MoveCooldown;

        public virtual void Move(Direction d, Map map, int time)
        {
            if (!CanMoveNow(time)) return;
            Dir = d;
            int nx = X, ny = Y;
            switch (d)
            {
                case Direction.Up: ny--; break;
                case Direction.Down: ny++; break;
                case Direction.Left: nx--; break;
                case Direction.Right: nx++; break;
            }
            if (map.IsInside(nx, ny) && map.CanTankEnter(nx, ny))
            {
                X = nx; Y = ny;
                lastMoveTime = time;
            }
        }

        public virtual Bullet? TryShoot()
        {
            int bx = X, by = Y;
            switch (Dir)
            {
                case Direction.Up: by--; break;
                case Direction.Down: by++; break;
                case Direction.Left: bx--; break;
                case Direction.Right: bx++; break;
            }
            return new Bullet(bx, by, Dir, this);
        }

        public void Damage(int dmg)
        {
            Health -= dmg;
        }

        public bool IsAlive => Health > 0;
    }
}
