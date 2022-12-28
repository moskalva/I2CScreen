
using System;
using System.Collections.Generic;
using System.Device.I2c;
using Iot.Device.Board;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Commands.Ssd1306Commands;

public class Ssd1306E2CScreen : IDisposable
{
    private static readonly Logger Log = Logger.Get();
    const int TotalPages = 8;
    const int BitsInByte = 8;
    bool isInitialized = false;

    private Ssd1306 device;
    private readonly ScreenSize screenSize;
    private byte[] displayBuffer;

    int bytesInRow => (int)screenSize.Horizontal / BitsInByte;
    int rowsInPage => (int)screenSize.Vertical / TotalPages;
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

    public void ClearDisplay()
    {
        Log.Info($"Clear whole buffer '{this.displayBuffer.Length}'");
        // Cleanup buffer
        for (int y = 0; y < displayBuffer.Length; y++)
        {
            Array.Fill(displayBuffer, byte.MinValue);
        }
        UpdateFullScreenFromBuffer();
    }

    public void ShowLine()
    {
        for (uint x = 0; x < 32; x++)
        {
            SetPixel(x, x, true);
        }
        UpdateFullScreenFromBuffer();
    }

    private void SetPixel(uint x, uint y, bool isSet = true)
    {
        var columnIndex = x;
        var rowIndex = y / BitsInByte;
        var byteIndex = (this.screenSize.Horizontal * rowIndex) + columnIndex;

        var newValue = 1 << (int)(y % BitsInByte);
        var currentValue = this.displayBuffer[byteIndex];
        int updatedValue = isSet
            ? currentValue | newValue
            : currentValue ^ newValue;
        this.displayBuffer[byteIndex] = (byte)updatedValue;
    }

    private void UpdateFullScreenFromBuffer()
    {
        UpdateSection(new ScreenSection
        {
            StartPage = 0,
            EndPage = 7,
            StartColumn = 0,
            Width = this.screenSize.Horizontal,
        }, this.displayBuffer);
    }

    private void UpdateSection(ScreenSection section, Span<byte> data)
    {
        // validate section is within screen
        device.SendCommand(new SetColumnAddress((byte)section.StartColumn, (byte)(section.StartColumn + section.Width - 1)));
        device.SendCommand(new SetPageAddress((PageAddress)section.StartPage, (PageAddress)section.EndPage));
        device.SendData(data);
    }

    public void Dispose()
    {
        Log.Warning($"Disposing");
        device.SendCommand(new SetDisplayOff());

        this.device.Dispose();
    }
}



