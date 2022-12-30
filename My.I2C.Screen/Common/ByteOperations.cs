

public static class ByteOperations
{
    public const int BitsInByte = 8;

    public static int SingleBitSet(uint position) => 0x00000001 << (int)position;

    public static int FlipBits(int value) => value ^ 0xFF;

    public static byte SetBit(byte value, uint position) => (byte)((int)value | SingleBitSet(position));
    public static byte UnSetBit(byte value, uint position) => (byte)((int)value & FlipBits(SingleBitSet(position)));
}