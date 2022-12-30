

using System;
using static ByteOperations;

public class ScreenSectionData
{
    private byte[] data;

    public ScreenSectionData(uint rowsNumber, uint width)
    {
        if (rowsNumber == 0)
            throw new ArgumentException($"Section rows number cannot be 0");
        if (width == 0)
            throw new ArgumentException($"Section width cannot be 0");

        this.RowsNumber = rowsNumber;
        this.Width = width;
        this.data = new byte[rowsNumber * width];
    }
    public uint RowsNumber { get; }
    public uint Width { get; }
    public uint Height => RowsNumber * BitsInByte;

    public SectionAddresingMode AddresingMode => SectionAddresingMode.Horizontal;

    public Span<byte> Data => this.data.AsSpan();

    public void Clear() => Array.Fill(this.data, byte.MinValue);

    public void SetPixel(uint x, uint y, bool isSet = true)
    {
        var columnIndex = x;
        var rowIndex = y / BitsInByte;
        var bitIndex = y % BitsInByte;
        var byteIndex = (this.Width * rowIndex) + columnIndex;

        if (columnIndex > Width)
            throw new ArgumentException("Pixel column is outside of section width");

        if (rowIndex > RowsNumber)
            throw new ArgumentException("Pixel row is outside of section rows number");

        var currentValue = this.data[byteIndex];
        var updatedValue = isSet
            ? SetBit(currentValue, bitIndex)
            : UnSetBit(currentValue, bitIndex);
        this.data[byteIndex] = updatedValue;
    }
}