namespace Tanki
{
    class Bullet(int x, int y, Direction dir, Tank owner)
    {
        public int X = x;
        public int Y = y;
        public Direction Dir = dir;
        public Tank Owner = owner;
        public bool Active = true;

        public void Move()
        {
            switch (Dir)
            {
                case Direction.Up: Y--; break;
                case Direction.Down: Y++; break;
                case Direction.Left: X--; break;
                case Direction.Right: X++; break;
            }
        }
    }
}
