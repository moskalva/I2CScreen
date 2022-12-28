using System;
using System.Collections.Generic;
using System.Device.I2c;
using Iot.Device.Board;

public static class InterctiveConsoleI2EDeviceSelector
{
    public static int GetScreenDevice(I2cBus bus){
        var connectedAddresses = bus.PerformBusScan();
        PrintAvailableDevices(connectedAddresses);
        return AskDeviceAddress(connectedAddresses);
    }

    private static void PrintAvailableDevices(IList<int> connectedAddresses)
    {
        Console.WriteLine($"Connected devices list:");
        for (int i = 0; i < connectedAddresses.Count; i++)
        {
            Console.WriteLine($"{i}: {connectedAddresses[i]}");
        }
    }

    private static int AskDeviceAddress(IList<int> connectedAddresses)
    {
        for(int attempts = 5;attempts>0;attempts--)
        {
            Console.WriteLine($"Input connected device number");
            var input = Console.ReadLine();
            if (int.TryParse(input, out var deviceNumber) && deviceNumber >= 0 && deviceNumber < connectedAddresses.Count)
            {
                return connectedAddresses[deviceNumber];
            }
            Console.WriteLine($"Incorrect input: '{input}'");
        }

        throw new InvalidOperationException("Cannot determine device");
    }
}