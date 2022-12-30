using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Drawing;
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

        public static UInt16 RaspberryPiDefaultI2EBusAddress = 1;
        static void Main(string[] args)
        {
            UInt16 busAddress = args != null && args.Length > 0 && UInt16.TryParse(args[0], out var parsed)
                              ? parsed : RaspberryPiDefaultI2EBusAddress;
            var screenSize = new ScreenSize(128, 64);
            var fullScreen = new ScreenSection(ScreenSectionPosition.Zero, new ScreenSectionData(8, 128));
            var blankExample = new ScreenSection(new ScreenSectionPosition(2, 25), new ScreenSectionData(1, 45));
            var lineExample = new ScreenSection(new ScreenSectionPosition(2, 65), new ScreenSectionData(3, 32));
            WithScreen(busAddress, screenSize, screen =>
            {
                screen.Init();
                ClearDisplay(screen, fullScreen);

                PrintBlank(screen, blankExample);
                PrintLineChain(screen, lineExample,
                    new Point(0, 0),
                    new Point(22, 5),
                    new Point(4, 15),
                    new Point(16, 12),
                    new Point((int)lineExample.Data.Width - 1, (int)lineExample.Data.Height - 1),
                    new Point(0, (int)lineExample.Data.Height - 1),
                    new Point((int)lineExample.Data.Width - 1, 0),
                    new Point(0, 0));
                Console.ReadLine();
            });
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
                Console.ReadLine();
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
                    Log.Info($"a: {aI};b:{bI};c:{cI}");
                    var x = p1.X + (isAPositive ? -aI : aI);
                    var y = p1.Y + (isBPositive ? -bI : bI);
                    Log.Info($"x: {x};y:{y};");
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
                    Log.Info($"a: {aI};b:{bI};c:{cI}");
                    var x = p1.X + (isAPositive ? -aI : aI);
                    var y = p1.Y + (isBPositive ? -bI : bI);
                    Log.Info($"x: {x};y:{y};");
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
