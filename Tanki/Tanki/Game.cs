using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

namespace Tanki
{
    class Game
    {
        Map map;
        PlayerTank player = new(0, 0);
        List<EnemyTank> enemies = new();
        List<Tank> allTanks => new List<Tank> { player }.Concat(enemies).ToList();
        List<Bullet> bullets = new();
        int level = 1;
        static Random rnd = new();
        List<Map> availableMaps;
        List<string>? mapFilesContent;

        public Game(Map startingMap, List<Map> maps)
        {
            Console.CursorVisible = false;
            availableMaps = maps ?? new List<Map>();
            map = startingMap;
            mapFilesContent = LoadMapsTextContents();
            ShowHelp();
            StartLevel(level);
        }

        List<string>? LoadMapsTextContents()
        {
            try
            {
                string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
                if (!Directory.Exists(folder)) return null;
                var files = Directory.GetFiles(folder, "*.txt");
                List<string> list = new();
                foreach (var f in files) list.Add(File.ReadAllText(f));
                return list;
            }
            catch { return null; }
        }

        void ShowHelp()
        {
            Console.Clear();
            int w = Console.WindowWidth;
            List<string> lines = new()
            {
                "Tanki - Controls",
                "Arrow keys: move",
                "Space / Enter: shoot",
                "Esc: quit",
                "",
                "Press any key to start"
            };
            int startY = Math.Max(0, (Console.WindowHeight - lines.Count) / 2);
            for (int i = 0; i < lines.Count; i++)
            {
                Console.SetCursorPosition(Math.Max(0, (w - lines[i].Length) / 2), startY + i);
                Console.Write(lines[i]);
            }
            Console.ReadKey(true);
        }

        void ShowLevelIntro(int lvl)
        {
            Console.Clear();
            string text = $"Level {lvl}";
            string instr = "Press any key to begin";
            int w = Console.WindowWidth;
            int h = Console.WindowHeight;
            Console.SetCursorPosition(Math.Max(0, (w - text.Length) / 2), h / 2 - 1);
            Console.Write(text);
            Console.SetCursorPosition(Math.Max(0, (w - instr.Length) / 2), h / 2 + 1);
            Console.Write(instr);
            Console.ReadKey(true);
        }

        void StartLevel(int lvl)
        {
            if (availableMaps.Count > 0)
            {
                map = availableMaps[(lvl - 1) % availableMaps.Count];
            }
            else
            {
                map = Map.GenerateRandom(20, 9, seed: lvl);
            }

            ShowLevelIntro(lvl);

            bool placed = false;
            if (mapFilesContent != null && mapFilesContent.Count > 0)
            {
                var content = mapFilesContent[(lvl - 1) % mapFilesContent.Count];
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int y = 0; y < Math.Min(lines.Length, map.Height); y++)
                {
                    var line = lines[y];
                    for (int x = 0; x < Math.Min(line.Length, map.Width); x++)
                    {
                        if (line[x] == 'P')
                        {
                            player = new(x, y);
                            placed = true; break;
                        }
                    }
                    if (placed) break;
                }
            }

            if (!placed) player = new(map.Width / 2, map.Height / 2);

            enemies.Clear(); bullets.Clear();
            int enemyCount = Math.Min(6, lvl + 1);
            for (int i = 0; i < enemyCount; i++)
            {
                int ex, ey;
                int attempts = 0;
                do
                {
                    ex = rnd.Next(1, map.Width - 1);
                    ey = rnd.Next(1, map.Height - 1);
                    attempts++;
                    if (attempts > 200) break;
                } while (!map.CanTankEnter(ex, ey) || (ex == player.X && ey == player.Y));
                enemies.Add(new(ex, ey));
            }

            DrawFrame();
        }

        void DrawFrame()
        {
            int statusY = 0;
            Console.SetCursorPosition(0, statusY);
            string status = $"Level {level}  Player HP: {player.Health}  Enemies: {enemies.Count}    (Arrows to move, Space to shoot, Esc to quit)";
            int width = Math.Max(Console.WindowWidth, map.Width * 2 + 1);
            if (status.Length < width) status = status.PadRight(width);
            Console.Write(status);
            map.Draw(0, 2, allTanks, bullets);
        }

        public void Run()
        {
            var lastTick = Environment.TickCount;
            bool exit = false;
            while (!exit)
            {
                int now = Environment.TickCount;
                int dt = now - lastTick;
                if (dt < 30) { Thread.Sleep(5); continue; }
                lastTick = now;

                while (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape) { exit = true; break; }
                    if (key.Key == ConsoleKey.LeftArrow) player.Move(Direction.Left, map, now);
                    if (key.Key == ConsoleKey.RightArrow) player.Move(Direction.Right, map, now);
                    if (key.Key == ConsoleKey.UpArrow) player.Move(Direction.Up, map, now);
                    if (key.Key == ConsoleKey.DownArrow) player.Move(Direction.Down, map, now);
                    if (key.Key == ConsoleKey.Spacebar || key.Key == ConsoleKey.Enter)
                    {
                        var b = player.TryShoot();
                        if (b != null && map.IsInside(b.X, b.Y)) bullets.Add(b);
                    }
                }

                foreach (var e in enemies.Where(x => x.IsAlive)) e.UpdateAI(map, player, bullets, now);

                foreach (var b in bullets.Where(bb => bb.Active).ToArray())
                {
                    b.Move();
                    if (!map.IsInside(b.X, b.Y)) { b.Active = false; continue; }
                    var cell = map.GetCell(b.X, b.Y);
                    if (cell.Type == CellType.Wall)
                    {
                        map.HitCell(b.X, b.Y);
                        b.Active = false;
                        continue;
                    }

                    if (player.IsAlive && player.X == b.X && player.Y == b.Y && b.Owner != player)
                    {
                        player.Damage(1);
                        b.Active = false;
                        continue;
                    }
                    var hitEnemy = enemies.FirstOrDefault(en => en.IsAlive && en.X == b.X && en.Y == b.Y && b.Owner != en);
                    if (hitEnemy != null)
                    {
                        hitEnemy.Damage(1);
                        b.Active = false;
                        continue;
                    }
                }
                bullets.RemoveAll(bb => !bb.Active);
                enemies.RemoveAll(e => !e.IsAlive);

                if (!player.IsAlive)
                {
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("You died. Game Over. Press R to restart or Esc to exit.");
                    bool waiting = true;
                    while (waiting)
                    {
                        var k = Console.ReadKey(true);
                        if (k.Key == ConsoleKey.R) { level = 1; StartLevel(level); waiting = false; }
                        if (k.Key == ConsoleKey.Escape) return;
                    }
                }

                if (enemies.Count == 0)
                {
                    level++;
                    if (level > 3)
                    {
                        Console.Clear(); Console.WriteLine("You Win! All levels cleared."); Console.WriteLine("Press R to play again or Esc to exit.");
                        bool waiting = true;
                        while (waiting)
                        {
                            var k = Console.ReadKey(true);
                            if (k.Key == ConsoleKey.R) { level = 1; StartLevel(level); waiting = false; }
                            if (k.Key == ConsoleKey.Escape) return;
                        }
                    }
                    else StartLevel(level);
                }

                DrawFrame();
            }
        }
    }
}
