using System;

namespace Tanki
{
    class PlayerTank : Tank
    {
        public PlayerTank(int x, int y) : base(x, y, ConsoleColor.Yellow)
        {
            MoveCooldown = 120;
            Health = 100;
        }
    }
}
