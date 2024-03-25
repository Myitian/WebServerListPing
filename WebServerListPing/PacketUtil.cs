using System.Text;
namespace WebServerListPing;
public static class PacketUtil
{
    private const int VARINT_SEGMENT_BITS = 0x7F;
    private const int VARINT_CONTINUE_BIT = 0x80;

    public static void WriteByte(Stream stream, byte value)
    {
        stream.WriteByte(value);
    }

    public static void WriteUInt16(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    public static void WriteInt64(Stream stream, long value)
    {
        stream.WriteByte((byte)(value >> 56));
        stream.WriteByte((byte)(value >> 48));
        stream.WriteByte((byte)(value >> 40));
        stream.WriteByte((byte)(value >> 32));
        stream.WriteByte((byte)(value >> 24));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    public static void WriteString(Stream stream, string value)
    {
        WriteVarInt(stream, value.Length);
        byte[] buffer = Encoding.UTF8.GetBytes(value);
        stream.Write(buffer, 0, buffer.Length);
    }

    public static int ReadVarInt(Stream stream) => ReadVarInt(stream, out _);
    public static int ReadVarInt(Stream stream, out int len)
    {
        int value = 0;
        int position = 0;
        int currentByte;
        len = 0;
        while (true)
        {
            currentByte = stream.ReadByte();
            if (currentByte < 0) throw new EndOfStreamException();
            value |= (currentByte & VARINT_SEGMENT_BITS) << position;
            len++;
            if ((currentByte & VARINT_CONTINUE_BIT) == 0) break;

            position += 7;

            if (position >= 32) throw new OverflowException("VarInt is too big");
        }

        return value;
    }
    public static void WriteVarInt(Stream stream, int value)
    {
        uint uval = (uint)value;
        while (true)
        {
            if ((uval & ~(uint)VARINT_SEGMENT_BITS) == 0)
            {
                stream.WriteByte((byte)uval);
                return;
            }

            stream.WriteByte((byte)((uval & VARINT_SEGMENT_BITS) | VARINT_CONTINUE_BIT));

            uval >>= 7;
        }
    }

    public static void WriteVarLong(Stream stream, long value)
    {
        ulong uval = (ulong)value;
        while (true)
        {
            if ((uval & ~(ulong)VARINT_SEGMENT_BITS) == 0)
            {
                stream.WriteByte((byte)uval);
                return;
            }

            stream.WriteByte((byte)((uval & VARINT_SEGMENT_BITS) | VARINT_CONTINUE_BIT));

            uval >>= 7;
        }
    }
}
