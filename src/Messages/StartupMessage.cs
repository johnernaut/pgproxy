namespace pgproxy.Messages;

public class StartupMessage : IReader
{
    public Dictionary<string, string> StartupParameters { get; }
    private uint ProtocolVersion { get; set; }

    public StartupMessage() => StartupParameters = new Dictionary<string, string>();

    public IReader ReadStartupMessage(byte[] data)
    {
        var message = new Message(data);
        var startUpCode = message.ReadUInt32();

        Console.WriteLine("Received: {0}", startUpCode);

        switch (startUpCode)
        {
            case 80877103:
                return new SSLRequest(startUpCode);
            case 80877102:
                return new CancelRequest(startUpCode);
            default:
                break;
        }

        var majorVersion = startUpCode >> 16;
        if (majorVersion < 3)
        {
            throw new Exception("PG protocol versions of < 3 are not supported.");
        }

        ProtocolVersion = startUpCode;

        while (true)
        {
            var paramString = message.ReadString(); //readString(data);
            if (String.IsNullOrEmpty(paramString))
            {
                break;
            }

            StartupParameters[paramString] = message.ReadString();
        }

        return this;
    }

    public byte[] Read()
    {
        var length = 4;

        // get length of all parameters to write back to buffer
        foreach (var parameter in StartupParameters)
        {
            length += parameter.Key.Length + 1;
            length += parameter.Value.Length + 1;
        }

        length += 1;
        var buffer = new byte[length];
        var message = new Message(buffer);

        message.WriteUInt32(ProtocolVersion);
        foreach (var parameter in StartupParameters)
        {
            message.WriteString(parameter.Key);
            message.WriteString(parameter.Value);
        }

        message.WriteByte(0);

        return message.Read();
    }
}