namespace Lando.LowLevel.ResultsTypes
{
	internal class ApduResponse
	{
		public int ReturnCode { get; set; }
		public byte[] RecvBuff = new byte[263];
		public int ResponseLength { get; set; }
	}
}