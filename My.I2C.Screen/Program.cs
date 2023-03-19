using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Board;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Cmds = Iot.Device.Ssd13xx.Commands.Ssd1306Commands;

namespace My.E2C.Screen
{
    class Program
    {
        private static readonly Logger Log = Logger.Get();

        public static UInt16 RaspberryPiDefaultI2EBusAddress = 1;
        static void Main(string[] args)
        {
            UInt16 busAddress = args != null && args.Length > 0 && UInt16.TryParse(args[0], out var parsed)
                              ? parsed : RaspberryPiDefaultI2EBusAddress;
            var screenSize = new ScreenSize(128, 64);
            var fullScreen = new ScreenSection(ScreenSectionPosition.Zero, new ScreenSectionData(8, 128));
            var blankExample = new ScreenSection(new ScreenSectionPosition(2, 25), new ScreenSectionData(1, 45));
            var lineExample = new ScreenSection(new ScreenSectionPosition(2, 65), new ScreenSectionData(3, 32));
            var textExample = new ScreenSection(new ScreenSectionPosition(0, 0), new ScreenSectionData(2, 128));
            WithScreen(busAddress, screenSize, screen =>
            {
                screen.Init();
                ClearDisplay(screen, fullScreen);
                TryDifferentFonts(screen, textExample);
                Console.ReadLine();
            });
        }

        private static void TryDifferentFonts(Ssd1306E2CScreen screen, ScreenSection section){
            var fonts = Directory.GetFiles("/usr/share/fonts/", "*.otf", SearchOption.AllDirectories);
            foreach(var font in fonts){
                ClearDisplay(screen, section);
                PrintBorder(screen, section);
                PrintText(screen, section, "Hello World!", font);
                Log.Info($"Font: '{font}'");
                Console.ReadLine();
            }
        }

        private static void PrintText(Ssd1306E2CScreen screen, ScreenSection section, string text, string fontPath)
        {
            var bmp = new Bitmap((int)section.Data.Width, (int)section.Data.Height);
            var rect = new RectangleF(0, 0, bmp.Width, bmp.Height);
            var format = new StringFormat(){
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.Character,
            };
            var graphics = Graphics.FromImage(bmp);
            var fonts = new PrivateFontCollection();
            fonts.AddFontFile(fontPath);
            var fontFamily = fonts.Families.FirstOrDefault();
            if (fontFamily is default(FontFamily))
            {
                throw new InvalidCastException("Cant find font");
            }
            
            var font = new Font(fontFamily, bmp.Height/2);
            graphics.DrawString(text, font, Brushes.Black, rect, format);
            graphics.Flush();
            
            for (uint x = 0; x < rect.Width; x++)
                for (uint y = 0; y < rect.Height; y++)
                {
                    var pixel = bmp.GetPixel((int)x, (int)y);
                    if (pixel.A > byte.MaxValue * 0.1)
                    {
                        section.Data.SetPixel(x, y);
                    }
                }
            screen.UpdateSection(section);
        }

        private static void PrintBorder(Ssd1306E2CScreen screen, ScreenSection section)
        {
            PrintLineChain(screen, section,
                                new Point(0, 0),
                                new Point((int)section.Data.Width - 1, 0),
                                new Point((int)section.Data.Width - 1, (int)section.Data.Height - 1),
                                new Point(0, (int)section.Data.Height - 1),
                                new Point(0, 0));
        }

        private static void PrintLineChain(Ssd1306E2CScreen screen, ScreenSection section, params Point[] points)
        {
            if (points == null || points.Length < 2)
                throw new ArgumentException("Provide at least 2 dots of chain");
            var start = points[0];
            for (int i = 1; i < points.Length; i++)
            {
                var end = points[i];
                PrintLine(screen, section, start, end);
                start = end;
            }
        }

        private static void PrintLine(Ssd1306E2CScreen screen, ScreenSection section, Point p1, Point p2)
        {
            var Log = Logger.Get();
            var a = Math.Abs(p1.X - p2.X);
            var b = Math.Abs(p1.Y - p2.Y);
            var c = Math.Sqrt(a * a + b * b);
            var angleA = a / c;
            var angleB = b / c;
            var isALonger = a > b;
            var isAPositive = p1.X > p2.X;
            var isBPositive = p1.Y > p2.Y;

            if (isALonger)
            {
                for (int i = 0; i < a; i++)
                {
                    var aI = a - i;
                    var cI = aI / angleA;
                    var bI = Math.Sqrt(cI * cI - aI * aI);
                    
                    var x = p1.X + (isAPositive ? -aI : aI);
                    var y = p1.Y + (isBPositive ? -bI : bI);
                    
                    section.Data.SetPixel((uint)x, (uint)y);
                }
            }
            else
            {
                for (int i = 0; i < b; i++)
                {
                    var bI = b - i;
                    var cI = bI / angleB;
                    var aI = Math.Sqrt(cI * cI - bI * bI);
                    
                    var x = p1.X + (isAPositive ? -aI : aI);
                    var y = p1.Y + (isBPositive ? -bI : bI);
                    
                    section.Data.SetPixel((uint)x, (uint)y);
                }
            }

            screen.UpdateSection(section);
        }

        private static void PrintBlank(Ssd1306E2CScreen screen, ScreenSection section)
        {
            var pixelsInLine = Math.Min(section.Data.Width, section.Data.Height);
            var xPrecision = section.Data.Width / (decimal)pixelsInLine;
            var yPrecision = section.Data.Height / (decimal)pixelsInLine;
            for (uint i = 0; i < pixelsInLine; i++)
            {
                var x = (uint)(i * xPrecision);
                var y = (uint)(i * yPrecision);
                var yRevers = section.Data.Height - y - 1;
                section.Data.SetPixel(x, y);
                section.Data.SetPixel(x, yRevers);
            }
            screen.UpdateSection(section);
        }

        private static void ClearDisplay(Ssd1306E2CScreen screen, ScreenSection section)
        {
            section.Data.Clear();
            screen.UpdateSection(section);
        }

        private static void WithScreen(ushort busAddress, ScreenSize size, Action<Ssd1306E2CScreen> action)
        {
            var disposables = new DisposableChain();
            try
            {
                var bus = disposables.Add(I2cBus.Create(busAddress));
                var screenAddress = InterctiveConsoleI2EDeviceSelector.GetScreenDevice(bus);

                var device = disposables.Add(bus.CreateDevice(screenAddress));
                var screen = disposables.Add(new Ssd1306(device));
                var ssd1306E2CScreen = disposables.Add(new Ssd1306E2CScreen(size, screen));
                action(ssd1306E2CScreen);
            }
            finally
            {
                // Dispose created resources in case of mid setup error
                disposables.Dispose();
            }
        }
    }

}
