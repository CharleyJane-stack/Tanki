using System;
using System.Collections.Generic;
using System.IO;

namespace Tanki
{
    enum CellType { Empty, Wall, Water }

    class Cell(CellType type, int durability = 0)
    {
        public CellType Type { get; set; } = type;
        public int Durability { get; set; } = durability;
    }

    class Map
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private readonly Cell[,] cells;

        public Map(string[] lines)
        {
            Height = lines.Length;
            Width = lines[0].Length;
            cells = new Cell[Width, Height];
            for (int y = 0; y < Height; y++)
            {
                var line = lines[y];
                for (int x = 0; x < Width; x++)
                {
                    char c = x < line.Length ? line[x] : '.';
                    // Use switch expression to initialize cells
                    cells[x, y] = c switch
                    {
                        '#' => new Cell(CellType.Wall, 3),
                        'B' => new Cell(CellType.Wall, 1),
                        '~' => new Cell(CellType.Water),
                        _ => new Cell(CellType.Empty)
                    };
                }
            }
        }

        public static Map? LoadFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            var lines = File.ReadAllLines(path);
            return new Map(lines);
        }

        public static List<Map> LoadMapsFromFolder(string folder)
        {
            var maps = new List<Map>();
            if (!Directory.Exists(folder)) return maps;
            var files = Directory.GetFiles(folder, "*.txt");
            foreach (var f in files)
            {
                var m = LoadFromFile(f);
                if (m != null) maps.Add(m);
            }
            return maps;
        }

        public static Map GenerateRandom(int w, int h, int seed = 0)
        {
            var rnd = seed == 0 ? new Random() : new Random(seed);
            var lines = new string[h];
            for (int y = 0; y < h; y++)
            {
                var chars = new char[w];
                for (int x = 0; x < w; x++)
                {
                    if (y == 0 || x == 0 || y == h - 1 || x == w - 1) chars[x] = '#';
                    else
                    {
                        int r = rnd.Next(100);
                        if (r < 6) chars[x] = '#';
                        else if (r < 10) chars[x] = 'B';
                        else if (r < 14) chars[x] = '~';
                        else chars[x] = '.';
                    }
                }
                lines[y] = new string(chars);
            }
            return new Map(lines);
        }

        public bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public bool CanTankEnter(int x, int y)
        {
            if (!IsInside(x, y)) return false;
            var c = cells[x, y];
            return c.Type == CellType.Empty;
        }

        public bool IsLineClear(int x1, int y1, int x2, int y2, bool allowWaterBullet)
        {
            if (x1 != x2 && y1 != y2) return false;
            int dx = Math.Sign(x2 - x1);
            int dy = Math.Sign(y2 - y1);
            int x = x1 + dx, y = y1 + dy;
            while (x != x2 || y != y2)
            {
                if (!IsInside(x, y)) return false;
                var c = cells[x, y];
                if (c.Type == CellType.Wall) return false;
                if (c.Type == CellType.Water && !allowWaterBullet) return false;
                x += dx; y += dy;
            }
            return true;
        }

        public Cell GetCell(int x, int y) => cells[x, y];

        public void HitCell(int x, int y)
        {
            if (!IsInside(x, y)) return;
            var c = cells[x, y];
            if (c.Type == CellType.Wall)
            {
                c.Durability--;
                if (c.Durability <= 0) cells[x, y] = new Cell(CellType.Empty);
            }
        }

        public void Draw(int offsetX, int offsetY, List<Tank> tanks, List<Bullet> bullets)
        {
            Console.SetCursorPosition(offsetX, offsetY);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var cell = cells[x, y];
                    bool drawn = false;
                    var tank = tanks.Find(t => t.X == x && t.Y == y && t.IsAlive);
                    if (tank != null)
                    {
                        Console.ForegroundColor = tank.Color;
                        Console.Write(GetTankChar(tank));
                        Console.Write(' ');
                        Console.ResetColor();
                        drawn = true;
                    }
                    else
                    {
                        var b = bullets.Find(bb => bb.X == x && bb.Y == y && bb.Active);
                        if (b != null)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write('*'); Console.Write(' ');
                            Console.ResetColor();
                            drawn = true;
                        }
                    }

                    if (!drawn)
                    {
                        switch (cell.Type)
                        {
                            case CellType.Empty: Console.Write("  "); break;
                            case CellType.Wall:
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                // Use switch expression for durability display
                                Console.Write(cell.Durability switch
                                {
                                    >= 3 => "##",
                                    2 => "[]",
                                    _ => "{}"
                                });
                                Console.ResetColor();
                                break;
                            case CellType.Water:
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write("~~");
                                Console.ResetColor();
                                break;
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        private static char GetTankChar(Tank t) => t is PlayerTank ? 'P' : 'E';
    }
}
