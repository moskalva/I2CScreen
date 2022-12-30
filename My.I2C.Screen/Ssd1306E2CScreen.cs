
using System;
using System.Collections.Generic;
using System.Device.I2c;
using Iot.Device.Board;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using static ByteOperations;

public class Ssd1306E2CScreen : IDisposable
{
    public const int TotalPages = 8;
    private static readonly Logger Log = Logger.Get();
    bool isInitialized = false;

    private Ssd1306 device;
    private readonly ScreenSize screenSize;
    private byte[] displayBuffer;

    int bytesInRow => (int)screenSize.Horizontal;
    int rowsInPage => (int)screenSize.Vertical / TotalPages / BitsInByte;
    int bytesInPage => bytesInRow * rowsInPage * TotalPages;

    public Ssd1306E2CScreen(ScreenSize screenSize, Ssd1306 device)
    {
        this.screenSize = screenSize;
        this.device = device ?? throw new ArgumentNullException(nameof(device));
        this.displayBuffer = new byte[screenSize.Vertical * screenSize.Horizontal / BitsInByte];
    }

    public void Init()
    {
        // Turn screen off
        device.SendCommand(new SetDisplayOff());

        // Perform standart setup
        device.SendCommand(new SetMemoryAddressingMode(SetMemoryAddressingMode.AddressingMode.Horizontal));
        device.SendCommand(new SetMultiplexRatio());    //Set MUX ratio to N+1 MUX
        device.SendCommand(new SetDisplayOffset());
        device.SendCommand(new SetDisplayStartLine());
        device.SendCommand(new SetSegmentReMap());          //Set Segment Re-map  RESET
        device.SendCommand(new SetComOutputScanDirection(true));          //Set COM Output Scan Direction 0=normal mode
        device.SendCommand(new SetComPinsHardwareConfiguration());    //Set COM Pins Hardware Configuration 
        device.SendCommand(new SetContrastControlForBank0(30));    // Set Contrast Control for BANK0 
        device.SendCommand(new EntireDisplayOn(false));//Entire Display ON 
        device.SendCommand(new SetNormalDisplay());          //Set A6 Normal / A7 Inverse Display
        device.SendCommand(new SetDisplayClockDivideRatioOscillatorFrequency());    //Set Display Clock Divide Ratio/Oscillator Frequency 
        device.SendCommand(new SetPreChargePeriod());    //Set Pre-charge Period
        device.SendCommand(new SetVcomhDeselectLevel(SetVcomhDeselectLevel.DeselectLevel.Vcc0_77));    //Set VCOMH Deselect Level ???
        device.SendCommand(new SetChargePump(true));    //Disable chargepump(RESET) 

        // Turn screen on
        device.SendCommand(new SetDisplayOn());
        device.SendCommand(new DeactivateScroll());
        device.SendCommand(new SetDisplayStartLine());

        isInitialized = true;
    }

    public void UpdateSection(ScreenSection section)
    {
        var startPage = Math.Ceiling(section.Position.Row / (decimal)rowsInPage);
        var endPage = startPage + Math.Ceiling(section.Data.RowsNumber / (decimal)rowsInPage);
        var startColumn = section.Position.Column;
        var endColumn = section.Position.Column + section.Data.Width - 1;

        if(startPage > TotalPages)
            throw new ArgumentException($"Provided section start page is outside of screen area");
        if(endPage > TotalPages)
            throw new ArgumentException($"Provided section end page is outside of screen area");

        if(section.Position.Column > this.screenSize.Horizontal)
            throw new ArgumentException($"Provided section start column is outside of screen area");
        if(endColumn > this.screenSize.Horizontal)
            throw new ArgumentException($"Provided section end column is outside of screen area");
        Log.Info($"Update Screen startpage: {startPage}, endPage: {endPage}, startColumn: {startColumn}, endColumn: {endColumn}, bytes: {section.Data.Data.Length}");
        device.SendCommand(new SetColumnAddress((byte)startColumn, (byte)endColumn));
        device.SendCommand(new SetPageAddress((PageAddress)(startPage), (PageAddress)(endPage)));
        device.SendData(section.Data.Data);
    }

    public void Dispose()
    {
        device.SendCommand(new SetDisplayOff());

        this.device.Dispose();
    }
}



