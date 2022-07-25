namespace pgproxy;

public class SSLRequest : IReader
{
    public UInt32 RequestCode { get; set; }

    public SSLRequest(UInt32 requestCode) => RequestCode = requestCode;

    public byte[] Read()
    {
        throw new NotImplementedException();
    }
}