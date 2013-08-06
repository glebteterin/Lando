namespace Lando.LowLevel.ResultsTypes
{
	internal class ReceiveCardIdResult : ApduOperationResult
	{
		public byte[] Bytes { get; set; }

		public bool IsCompletelySuccessful
		{
			get { return IsSuccessful && IsApduCommandSuccessful; }
		}

		public ReceiveCardIdResult(OperationResult operationResult)
			: base(operationResult)
		{
		}
	}
}