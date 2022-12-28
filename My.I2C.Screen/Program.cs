using System;
using System.Collections.Generic;
using System.Device.I2c;
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
            var screenSize = new ScreenSize(128,64);

            WithScreen(busAddress, screenSize, screen =>
            {
                screen.Init();
                screen.ClearDisplay();

                screen.ShowLine();
                Console.ReadLine();
            });
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
