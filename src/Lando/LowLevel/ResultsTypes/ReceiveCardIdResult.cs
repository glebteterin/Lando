namespace Lando.LowLevel.ResultsTypes
{
	internal class ReceiveCardIdResult : ApduOperationResult
	{
		public byte[] Bytes { get; set; }

		public ReceiveCardIdResult(ApduOperationResult apduOperationResult)
			: base(apduOperationResult, apduOperationResult.IsApduCommandSuccessful)
		{
		}
	}
}