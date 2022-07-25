using System.Buffers.Binary;
using System.Text;

namespace pgproxy;

// IReader is an interface that message types implement to write their
// data back to a connection
public interface IReader
{
    public byte[] Read();
}

public class Message : IReader
{
    public byte[]? T { get; set; }
    private byte[] Data { get; set; }
    private int Offset { get; set; }

    public Message(byte[] data) => Data = data;

    public uint ReadUInt32()
    {
        var code = BinaryPrimitives.ReadUInt32BigEndian(Data[Offset..(Offset + 4)]);
        Offset += 4;
        return code;
    }

    public string ReadString()
    {
        var end = Offset;
        var max = Data.Length;

        while (Data[end] != 0 && end != max)
        {
            end++;
        }

        var paramString = Encoding.Default.GetString(Data[Offset..end]);

        Offset = end + 1;

        return paramString;
    }

    public void WriteUInt32(uint val)
    {
        BinaryPrimitives.WriteUInt32BigEndian(Data.AsSpan()[Offset..(Offset + 4)], val);
        Offset += 4;
    }

    public void WriteString(string val)
    {
        var bytes = Encoding.Default.GetBytes(val);
        foreach (var b in bytes)
        {
            WriteByte(b);
        }

        WriteByte(0);
    }

    public void WriteByte(byte val)
    {
        Data[Offset] = val;
        Offset++;
    }

    public byte[] Read()
    {
        var length = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(length, (uint)Data.Length + 4);

        var s = new MemoryStream();
        // s.Write(T);
        s.Write(length);
        s.Write(Data);

        return s.ToArray();
    }
}