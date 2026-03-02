using System;
using System.IO;
using System.Collections.Generic;
using Tanki;

namespace Tanki.Core
{
    public static class Program
    {
        public static void Main()
        {
            Console.Title = "Tanki - Console";
            Console.CursorVisible = false;

            string mapsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
            var maps = Map.LoadMapsFromFolder(mapsFolder);
            Map startingMap = maps.Count > 0 ? maps[0] : Map.GenerateRandom(20, 9);

            var game = new Game(startingMap, maps);
            game.Run();
        }
    }
}
