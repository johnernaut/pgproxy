namespace pgproxy
{
	public class CancelRequest : IReader
	{
		public UInt32 RequestCode { get; set; }

		public CancelRequest(UInt32 requestCode) => RequestCode = requestCode;

		public byte[] Read()
		{
			throw new NotImplementedException();
		}
	}
}