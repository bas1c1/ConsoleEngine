using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;

namespace ConsoleCore.Engine
{
    public static class Extensions
    {
        public static void ToGrayscale(this Bitmap bitmap)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    int avg = (pixel.R + pixel.G + pixel.B) / 3;
                    bitmap.SetPixel(x, y, Color.FromArgb(pixel.A, avg, avg, avg)); 
                }
            }
        }
    }
    public class Engine
    {
        //Object[] memory = new Object[] {  };
        public static Object[,] mem = {

        };

        public static List<List<int>> coords = new List<List<int>>();

        public static char pixel_char = '●';

        public static char pixel()
        {
            return pixel_char;
        }

        public static void fix_encoding()
        {
            Console.OutputEncoding = Encoding.Unicode;
        }

        public static string pixel_string(int size)
        {
            string end_str = "";
            for (int i = 0; i < size; i++)
            {
                end_str += pixel();
            }
            return end_str;
        }

        public class Engine3D : Engine   
        {
            public static string map =
            "111111111111111111111" +
            "100000010000000000001" +
            "110000000100000111001" +
            "100000000000000000001" +
            "100000111000000000001" +
            "100001100000000001001" +
            "100000000000000010001" +
            "101000110000000000111" +
            "100000000000000000001" +
            "101000000000000000001" +
            "111111111111111111111";

            public static double px = 2, py = 2, angle = 0, fov = Math.PI / 3, raySpeed = 0.1f;
            public static int w = 140, h = 41, mapW = 21, mapH = 9, playerSpeed = 10;

            public static char[] buffer;


            public static void Input(double delta)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    switch (k)
                    {
                        case ConsoleKey.A:
                            angle += 1f * delta;
                            break;
                        case ConsoleKey.D:
                            angle -= 1f * delta;
                            break;
                        case ConsoleKey.W:
                            px += Math.Sin(angle) * playerSpeed * delta;
                            py += Math.Cos(angle) * playerSpeed * delta;
                            if (map[(int)py * mapW + (int)px] == '1')
                            {
                                px -= Math.Sin(angle) * playerSpeed * delta;
                                py -= Math.Cos(angle) * playerSpeed * delta;
                            }
                            break;
                        case ConsoleKey.S:
                            px -= Math.Sin(angle) * playerSpeed * delta;
                            py -= Math.Cos(angle) * playerSpeed * delta;
                            if (map[(int)py * mapW + (int)px] == '1')
                            {
                                px += Math.Sin(angle) * playerSpeed * delta;
                                py += Math.Cos(angle) * playerSpeed * delta;
                            }
                            break;
                    }
                }
            }
            //string map, double px, double py, double angle, int w, int h, double fov, int mapW, int mapH, int playerSpeed, double raySpeed
            public static void init3D()
            {
                buffer = new char[w * h];
                Engine.cursor(false);

                Console.SetWindowSize(w, h);
                Console.SetBufferSize(w, h);

                var currdate = DateTime.Now;

                while (true)
                {
                    var newTime = DateTime.Now;
                    var deltatime = (newTime - currdate).TotalSeconds;
                    currdate = DateTime.Now;
                    Input(deltatime);

                    double[] dists = new double[w];
                    for (int x = 0; x < w; x++)
                    {
                        double raydir = angle + fov / 2 - x * fov / w;
                        double rayx = Math.Sin(raydir);
                        double rayy = Math.Cos(raydir);

                        double dist = 0;
                        bool hit = false;
                        double deph = 20;

                        while (!hit && dist < deph)
                        {
                            dist += raySpeed;

                            int tx = (int)(px + rayx * dist);
                            int ty = (int)(py + rayy * dist);

                            if (tx < 0 || tx >= deph + px || ty < 0 || ty >= deph + py)
                            {
                                hit = true;
                                dist = deph;
                            }
                            else
                            {
                                if (map[ty * mapW + tx] == '1')
                                {
                                    hit = true;
                                }
                            }
                        }
                        dists[x] = dist; //Хай будет, можно будет сделать грани.

                        int wall = (int)(h / 2d - h * fov / dist);
                        int floor = h - wall;

                        for (int y = 0; y < h; y++)
                        {
                            if (y <= wall)
                            {
                                buffer[y * w + x] = ' ';
                            }
                            else if (y > wall && y <= floor)
                            {
                                char wallColour = ' ';

                                if (dist <= deph / 4f)
                                    wallColour = '█';
                                else if (dist <= deph / 3f)
                                    wallColour = '▓';
                                else if (dist <= deph / 2f)
                                    wallColour = '▒';
                                else if (dist <= deph / 1f)
                                    wallColour = '░';
                                else
                                {
                                    wallColour = '│';
                                }

                                buffer[y * w + x] = wallColour;
                            }
                            else
                            {
                                double floorDist = 1 - (y - h / 2d) / (h / 2f);
                                char floorChar = ' ';
                                if (floorDist <= 0.2f)
                                    floorChar = '*';
                                else if (floorDist <= 0.50f)
                                    floorChar = '+';
                                else
                                     if (floorDist <= 0.75f)
                                    floorChar = '-';
                                else
                                {
                                    floorChar = '.';
                                }

                                buffer[y * w + x] = floorChar;
                            }
                        }

                    }



                    Console.SetCursorPosition(0, 0);
                    Console.Write(buffer);
                }
            }
        }

        public class Drawing : Engine
        {
            public static void draw_map(int weight, int height, char wall, char empty, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                for (int i = 0; i <= height; i++)
                {
                    if (i == 0 || i == height)
                    {
                        for (int j = 0; j <= weight; j++)
                        {
                            Console.Write(wall);
                        }
                    }
                    else
                    {
                        Console.Write(wall);
                        for (int c = 0; c <= weight - 2; c++)
                        {
                            Console.Write(empty);
                        }
                        Console.Write(wall);
                    }
                    Console.WriteLine();
                }
                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_map_at(int pos_x, int pos_y, int weight, int height, char wall, char empty, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                Console.SetCursorPosition(pos_x, pos_y);
                for (int i = 0; i <= height; i++)
                {
                    if (i == 0 || i == height)
                    {
                        for (int j = 0; j <= weight; j++)
                        {
                            Console.Write(wall);
                        }
                    }
                    else
                    {
                        Console.Write(wall);
                        for (int c = 0; c <= weight - 2; c++)
                        {
                            Console.Write(empty);
                        }
                        Console.Write(wall);
                    }
                    Console.SetCursorPosition(pos_x, pos_y + i + 1);
                }
                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_square(int pos_x, int pos_y, int size_x, int size_y, char wall, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                Console.SetCursorPosition(pos_x, pos_y);
                for (int i = 0; i < size_y; i++)
                {
                    Console.SetCursorPosition(pos_x, pos_y + i);
                    for (int j = 0; j < size_x; j++)
                    {
                        Console.Write(wall);
                    }
                }
                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_line(int pos_x, int pos_y, string line, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                Console.SetCursorPosition(pos_x + 1, pos_y);
                //cout << '\b' << " " << '\b';
                Console.Write("\b");
                Console.Write(" ");
                Console.Write("\b");
                int count = 0;
                foreach (char i in line)
                {
                    count += 1;
                }
                for (int i = 0; i < count; i++)
                {
                    Console.Write("\b");
                    Console.Write(" ");
                    Console.Write("\b");
                    //cout << line[i] << " ";
                    Console.Write(line[i]);
                    Console.Write(" ");
                }
                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_vertical_line(int pos_x, int pos_y, string line, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                Console.SetCursorPosition(pos_x + 1, pos_y);
                int count = 0;
                foreach (char i in line)
                {
                    count += 1;
                }
                for (int i = 0; i < count; i++)
                {
                    Console.Write("\b");
                    Console.Write(" ");
                    Console.Write("\b");
                    Console.Write(line[i]);
                    Console.Write(" ");
                    Console.SetCursorPosition(pos_x + 1, pos_y + i + 1);
                }
                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_diagonal_line(int pos_x, int pos_y, string line, int mode, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                int count;
                switch(mode)
                {
                    case 1:
                        Console.SetCursorPosition(pos_x + 1, pos_y);
                        count = 0;
                        foreach (char i in line)
                        {
                            count += 1;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            Console.Write("\b");
                            Console.Write(" ");
                            Console.Write("\b");
                            Console.Write(line[i]);
                            Console.Write(" ");
                            Console.SetCursorPosition(pos_x + 2 + i, pos_y + i + 1);
                        }
                        break;

                   case 2:
                        Console.SetCursorPosition(pos_x + 1, pos_y);
                        count = 0;
                        foreach (char i in line)
                        {
                            count += 1;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            Console.Write("\b");
                            Console.Write(" ");
                            Console.Write("\b");
                            Console.Write(line[i]);
                            Console.Write(" ");
                            Console.SetCursorPosition(pos_x - 0 - i, pos_y + i + 1);
                        }
                        break;

                    case 3:
                        Console.SetCursorPosition(pos_x + 1, pos_y);
                        count = 0;
                        foreach (char i in line)
                        {
                            count += 1;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            Console.Write("\b");
                            Console.Write(" ");
                            Console.Write("\b");
                            Console.Write(line[i]);
                            Console.Write(" ");
                            Console.SetCursorPosition(pos_x + 2 + i, pos_y - i - 1);
                        }
                        break;

                    case 4:
                        Console.SetCursorPosition(pos_x + 1, pos_y);
                        count = 0;
                        foreach (char i in line)
                        {
                            count += 1;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            Console.Write("\b");
                            Console.Write(" ");
                            Console.Write("\b");
                            Console.Write(line[i]);
                            Console.Write(" ");
                            Console.SetCursorPosition(pos_x - 0 - i, pos_y - i - 1);
                        }
                        break;
                }

                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_pixel_sphere(int pos_x, int pos_y, int r, int color)
            {
                Color.c_set_foreground_color(color);
                int per = 0;
                double r_in = r - 0.4;
                double r_out = r + 0.4;
                Console.SetCursorPosition(pos_x + 1, pos_y);
                for (double y = r + 1; y >= -r; --y)
                {
                    per++;
                    for (double x = -r; x < r_out; x += 0.5)
                    {
                        double value = x * x + y * y;
                        if (value >= r_in * r_in && value <= r_out * r_out)
                        {
                            //Console.Write(pixel());
                            draw_obj((int)x, (int)y, '@', color);
                        }
                        else if (true && value < r_in * r_in && value < r_out * r_out)
                        {
                            //Console.Write(pixel());
                            draw_obj((int)x, (int)y, '@', color);
                        }
                        else
                        {
                            Console.Write(' ');
                        }
                    }
                    Console.SetCursorPosition(pos_x, pos_y + (int)per - 1);
                }
                Color.c_set_foreground_color(15);
            }

            public static void draw_sphere(int pos_x, int pos_y, int r, char wall, int color)
            {
                Color.c_set_foreground_color(color);
                int per = 0;
                double r_in = r - 0.4;
                double r_out = r + 0.4;
                Console.SetCursorPosition(pos_x + 1, pos_y);
                for (double y = r+1; y >= -r; --y)
                {
                    per++;
                    for (double x = -r; x < r_out; x += 0.5)
                    {
                        double value = x * x + y * y;
                        if (value >= r_in * r_in && value <= r_out * r_out)
                        {
                            //Console.Write(wall);
                            draw_obj((int)pos_x + (int)x * 2, (int)pos_y + (int)y, '@', color);
                        }
                        else if (true && value < r_in * r_in && value < r_out * r_out)
                        {
                            //Console.Write(wall);
                            draw_obj((int)pos_x + (int)x * 2, (int)pos_y + (int)y, '@', color);
                        }
                        else
                        {
                            Console.Write(' ');
                        }
                    }
                    Console.SetCursorPosition(pos_x, pos_y + (int)per-1);
                }
                Color.c_set_foreground_color(15);
            }



            public static void draw_another_pixel_treugolnik(int pos_x, int pos_y, int size_y, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                Console.SetCursorPosition(pos_x, pos_y);
                int period = 1;
                for (int i = 0; i < size_y; ++i)
                {
                    Console.SetCursorPosition(pos_x - period, pos_y + i);

                    for (int j = 0; j < period; j++)
                    {
                        Console.Write(pixel());
                    }
                    period += 1;
                }
                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_another_treugolnik(int pos_x, int pos_y, int size_y, char wall, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                Console.SetCursorPosition(pos_x, pos_y);
                int period = 1;
                for (int i = 0; i < size_y; ++i)
                {
                    Console.SetCursorPosition(pos_x - period, pos_y + i);

                    for (int j = 0; j < period; j++)
                    {
                        Console.Write(wall);
                    }
                    period += 1;
                }
                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_pixel_romb(int pos_x, int pos_y, int size_y, int color)
            {
                for (int i = 0; i < size_y; i++)
                {
                    draw_diagonal_line(pos_x + i, pos_y + size_y - i - 1, pixel_string(size_y), 1, color);
                }
                Color.c_set_foreground_color(15);
            }

            public static void draw_romb(int pos_x, int pos_y, int size_y, char wall, int color)
            {
                string a = "";
                for (int i = 0; i < size_y; i++)
                {
                    a += wall;
                }
                for (int i = 0; i < size_y; i++)
                {
                    draw_diagonal_line(pos_x + i, pos_y + size_y - i - 1, a, 1, color);
                }
                Color.c_set_foreground_color(15);
            }

            public static void draw_pixel_treugolnik(int pos_x, int pos_y, int size_y, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                Console.SetCursorPosition(pos_x, pos_y);
                int period = 1;
                for (int i = 0; i < size_y; ++i)
                {
                    Console.SetCursorPosition(pos_x - i, pos_y + i);

                    for (int j = 0; j < period; j++)
                    {
                        Console.Write(pixel());
                    }
                    period += 2;
                }
                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_treugolnik(int pos_x, int pos_y, int size_y, char wall, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                Console.SetCursorPosition(pos_x, pos_y);
                int period = 1;
                for (int i = 0; i < size_y; ++i)
                {
                    Console.SetCursorPosition(pos_x - i, pos_y + i);

                    for (int j = 0; j < period; j++)
                    {
                        Console.Write(wall);
                    }
                    period += 2;
                }
                Engine.Color.c_set_foreground_color(15);
            }

            public static void draw_pixel_square(int pos_x, int pos_y, int size_x, int size_y, int color)
            {
                Console.SetCursorPosition(pos_x, pos_y);
                for (int i = 0; i < size_y; i++)
                {
                    Console.SetCursorPosition(pos_x, pos_y + i);
                    for (int j = 0; j < size_x; j++)
                    {
                        draw_at('●', color);
                    }
                }
            }

            public static void draw_at(char obj, int color)
            {
                Color.c_set_foreground_color(color);
                Console.Write(obj);
                Color.c_set_foreground_color(15);
            }

            public static void draw_pixel(int pos_x, int pos_y, int color)
            {
                draw_obj(pos_x, pos_y, '●', color);
            }

            public static void draw_obj(int pos_x, int pos_y, char obj_char, int color)
            {
                Engine.Color.c_set_foreground_color(color);
                Console.SetCursorPosition(pos_x + 1, pos_y);
                Console.Write("\b");
                Console.Write(" ");
                Console.Write("\b");
                Console.Write(obj_char);
                Engine.Color.c_set_foreground_color(15);
            }

            public static void delete_obj(int pos_x, int pos_y)
            {
                Console.SetCursorPosition(pos_x + 1, pos_y);
                Console.Write("\b");
                Console.Write(" ");
                Console.Write("\b");
            }
        }

        public class Sprite : Engine
        {

            public class BitmapToASCIIConverter
            {
                // '.'
                private static readonly char[] _asciiTable = { ' ', ',', ':', '+', '*', '?', '%', 'S', '#', '@' };
                private static Bitmap _bitmap;

                //readonly 

                public BitmapToASCIIConverter(Bitmap bitmap)
                {
                    _bitmap = bitmap;
                }
                //static
                public char[][] Convert()
                {
                    
                    var result = new char[_bitmap.Height][];

                    for (int y = 0; y < _bitmap.Height; y++)
                    {
                        result[y] = new char[_bitmap.Width];

                        for (int x = 0; x < _bitmap.Width; x++)
                        {
                            int mapIndex = (int)Map(_bitmap.GetPixel(x, y).R, 0, 255, 0, _asciiTable.Length - 1);
                            result[y][x] = _asciiTable[mapIndex];
                        }
                    }

                    return result;
                }

                private static float Map(float valueToMap, float start1, float stop1, float start2, float stop2)
                {
                    return ((valueToMap - start1) / (stop1 - start1)) * (stop2 - start2) + start2;
                }
            }

            

            private const double WIDTH_OFFSET = 1.5;
            
            [STAThread]

            public static void generate(Bitmap bitmap, int x, int y, int width, bool onOff)
            {
                Console.SetCursorPosition(x, y);
                // '.'
                //char[] _asciiTable = { ' ', ',', ':', '+', '*', '?', '%', 'S', '#', '@' };

                bitmap = ResizeBitmap(bitmap, width);
                bitmap.ToGrayscale();
                
                var converter = new BitmapToASCIIConverter(bitmap);
                var rows = converter.Convert();
                List<string> toWrite = new List<string>();
                int n = 0;

                
                
                if (onOff) {
                    foreach (var row in rows) {
                        string row2 = "";
                        foreach (char a in row) {
                            row2 += a;
                        }
                        toWrite.Add(row2);
                        n += 1;
                        Console.Write(row2);
                        Console.SetCursorPosition(x, y + n);

                    }
                    string end = "";
                    foreach(string line in toWrite)
                    {
                        end += line + '\n';
                    }
                    File.WriteAllText("123.txt", end);
                }
                else
                {
                    foreach (var row in rows)
                    {
                        n += 1;
                        //Console.Write(row);
                        foreach (var ch in row)
                        {
                            Console.Write(' ');
                        }
                        Console.SetCursorPosition(x, y + n);
                    }
                }
            }

            private static Bitmap ResizeBitmap(Bitmap bitmap, int maxWidth)
            {
                //var maxWidth = 350
                var newHeight = bitmap.Height / WIDTH_OFFSET * maxWidth / bitmap.Width;
                if (bitmap.Width > maxWidth || bitmap.Height > newHeight)
                    bitmap = new Bitmap(bitmap, new Size(maxWidth, (int)newHeight));
                return bitmap;
            }
        }

        public class Keyboard : Engine
        {
            public static char get_key()
            {
                ConsoleKeyInfo key;
                int? num = null;
                key = Console.ReadKey(true);
                num = Convert.ToInt32(key.Key);
                if (num != null) return key.KeyChar;
                else return ' ';
            }
        }

        public class Color : Engine
        {
            public enum CharacterAttributes
            {
                FOREGROUND_BLUE = 0x0001,
                FOREGROUND_GREEN = 0x0002,
                FOREGROUND_RED = 0x0004,
                FOREGROUND_INTENSITY = 0x0008,
                BACKGROUND_BLUE = 0x0010,
                BACKGROUND_GREEN = 0x0020,
                BACKGROUND_RED = 0x0040,
                BACKGROUND_INTENSITY = 0x0080,
                COMMON_LVB_LEADING_BYTE = 0x0100,
                COMMON_LVB_TRAILING_BYTE = 0x0200,
                COMMON_LVB_GRID_HORIZONTAL = 0x0400,
                COMMON_LVB_GRID_LVERTICAL = 0x0800,
                COMMON_LVB_GRID_RVERTICAL = 0x1000,
                COMMON_LVB_REVERSE_VIDEO = 0x4000,
                COMMON_LVB_UNDERSCORE = 0x8000
            }

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetConsoleTextAttribute(
            IntPtr hConsoleOutput,
            int wAttributes);
            /* declaring the setconsoletextattribute function*/

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetStdHandle(int nStdHandle);

            public static void set_foreground_color(int color)
            {
                ConsoleColor[] consoleColors = {
            ConsoleColor.Black, ConsoleColor.DarkBlue, ConsoleColor.DarkGreen, ConsoleColor.DarkCyan, ConsoleColor.DarkRed,
            ConsoleColor.DarkMagenta, ConsoleColor.DarkYellow, ConsoleColor.Gray, ConsoleColor.Blue, ConsoleColor.Green,
            ConsoleColor.Cyan, ConsoleColor.Red, ConsoleColor.Magenta, ConsoleColor.Yellow, ConsoleColor.White
            };

                Console.ForegroundColor = consoleColors[color];
            }

            public static void c_set_foreground_color(int color)
            {
                IntPtr hOut; /* declaring variable to get handle*/
                hOut = GetStdHandle(-11);

                SetConsoleTextAttribute(hOut, color);
            }

            public static void reset_color()
            {
                Console.ResetColor();
            }
        }

        public static void cursor(bool onOff)
        {
            Console.CursorVisible = onOff;
        }

        public static void sleep(int mills)
        {
            Thread.Sleep(mills);
        }

        public static void clear()
        {
            System.Console.Clear();
        }

        public static void end()
        {
            Engine.clear();

            string titri = @"
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@##########@@@@@@@@@@@@@@@@@@@@@@@@@###S##@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@##SS#@@@@@@@@@@@@@@@@@@@@@@@@###S##@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@####@@@@@@@@@@@@@@
@@#S?*****?%#@@@@@@@@@@@@@@@@@@@@@@@@@#S?*%#@@@@@@@@@@@@@@@@@@@@@@@@@@@@@##%**S@@@@@@@@@@@@@@@@@@@@@@@@#S**%##@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@##S%%S#@@@@@@@@@@@@@
@@#%++?????%#@@@@@@@@@@@@@@@@@@@@@@@@@######@@@@@@@@@@@@@@@@@@@@@@@@@@@@@##%**S@@@@@@@@@@@@@@@@@@@@@@@@#S**%##@@@@@@@@@@@@@@@@@@@@@@@@@@@@##S?*++%#@@@@@@@@@@@@@
@@#%+*S#@@@@@@##########@@@@###########################@@@@########@@@@@@##%**S######@#####@@@#####@@@@#S**%#######@@@########@@@##########%***++%#@@@@#########
@@#%+*S#######S??%??*?%S#@##S?**??%??S#S%?S##%??%??*?%S#@##S??**?S##@@@@@##%**%%?**?S##%?%##@##%?%#@@@@#S*+?%?**?%S####%?***?S###S%?**?%####S#S?+%#@@@##S?***?S#
@@#%++%%SSS###%++*?%%*+?S##%+*?%%?++*S#S*+%##%++*?%?*+?###%**?%?**%#@@@@@##%++*?%%?+*%S%**S#@#S**%#@@@@#S*++*?%?*+?S##%*?%%*+*S#S?+?%%?%S#@@@#S?+%#@@#S?**?%%%S#
@@#%+:++***S##%++%#@#S*+%#S*+S#@##%+*S#S*+%##%+*%#@#S+*S#%**S###%**S@@@@@##%++%####%+*S#?*?###?*?##@@@@#S*+?S#@#S?*%#######S**%#S**%######@@@#S?+%#@##?*?S#@@@@@
@@#%+*S#######%+*S#@#S*+%#%*?##@##%**S#S*+%##%+*S#@#S*+%#?++*****+*S@@@@@##%**S@@@#S**S#S**%#%**S#@@@@@#S*+%##@##%*?##S%???*++%##%***?S###@@@#S?+%#@#S**%#@@@@@@
@@#%+*S#@@@@##%+*S#@#S*+%#%*?##@##%**S#S*+%##%+*S#@#S*+%#?+*%SSSSSS#@@@@@##%**S@@@#S**S##%*?%?*%#@@@@@@#S*+%##@##?*?SS**?%%%**%####%?*+?S#@@@#S?+%#@#S**%##@@@@#
@@#%+*S#######%+*S#@#S*+%#%*+%###S?+*S#S*+%##%+*S#@#S*+%#%**S#######@@@@@##%++%###S?+?S@#S?***?##@@@@@@#S*+*S###%**%S?*?S##S**%######S**%#@@@#S?+%#@#S?**S######
@@#%++*?????S#%**S#@#S*+%##%*+*??**+*S#S**%##%*?S#@#S**%##%**?????S#@@@@@##%++*???**?S#@@#%*+*S#@@@@@@@#S*++*??***%##%*+*???**%#S?*???*?S#@@@#S?+%#@@#S?**????%S
@@#S%???????S#S%%##@##%%S###S%??%%%**S##%%S##S%%##@##%%S###S%???%%##@@@@@@#S%%%%??%S##@@@#S?+?##@@@@@@@##%%%%%??%S####S%??%S%%S##%???%S###@@@##%%S#@@@##S%???%S%
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@##?+?##@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@##%**S#@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@#S?
@@@@@@@@@@@@@@@@@@@@@@@@@##SSSSSS?**%#@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@###S%**%#@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@##
@@@@@@@@@@@@@@@@@@@@@@@@@##%******?S#@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@##%***%##@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@####SSSS###@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@##SS###@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
";
            Console.WriteLine(titri);
        }

        public static int randint(int ot, int doo)
        {
            Random rnd = new Random();
            return rnd.Next(ot, doo);
        }

        public static bool check_collision(int pos_x1, int pos_y1, int pos_x2, int pos_y2)
        {
            return pos_x1 == pos_x2 && pos_y1 == pos_y2;
        }
    }
}
