using System;
using System.Collections.Generic;

namespace Tanki
{
    class EnemyTank : Tank
    {
        private static readonly Random rnd = new();
        private int lastAiTime = 0;
        private readonly int aiInterval = 350;

        public EnemyTank(int x, int y) : base(x, y, ConsoleColor.Red)
        {
            MoveCooldown = 300;
            Health = 1;
        }

        public void UpdateAI(Map map, PlayerTank player, List<Bullet> bullets, int time)
        {
            if (time - lastAiTime < aiInterval) return;
            lastAiTime = time;

            if (player.X == X)
            {
                int sign = Math.Sign(player.Y - Y);
                if (map.IsLineClear(X, Y, X, player.Y, allowWaterBullet: true))
                {
                    Dir = sign > 0 ? Direction.Down : Direction.Up;
                    bullets.Add(new Bullet(X, Y + (sign > 0 ? 1 : -1), Dir, this));
                    return;
                }
            }
            if (player.Y == Y)
            {
                int sign = Math.Sign(player.X - X);
                if (map.IsLineClear(X, Y, player.X, Y, allowWaterBullet: true))
                {
                    Dir = sign > 0 ? Direction.Right : Direction.Left;
                    bullets.Add(new Bullet(X + (sign > 0 ? 1 : -1), Y, Dir, this));
                    return;
                }
            }

            var arr = (Direction[])Enum.GetValues(typeof(Direction));
            for (int i = 0; i < arr.Length; i++)
            {
                int j = rnd.Next(i, arr.Length);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            foreach (var d in arr)
            {
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
                    Move(d, map, time);
                    break;
                }
            }
        }
    }
}
