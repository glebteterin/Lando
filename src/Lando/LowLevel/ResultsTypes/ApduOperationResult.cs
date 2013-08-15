namespace Lando.LowLevel.ResultsTypes
{
	public class ApduOperationResult : OperationResult
	{
		public bool IsApduCommandSuccessful { get; private set; }

		public bool IsCompletelySuccessful
		{
			get { return IsSuccessful && IsApduCommandSuccessful; }
		}

		public ApduOperationResult(OperationResult operationResult, bool isApduCommandSuccessful)
			: base(
			operationResult.IsSuccessful,
			operationResult.StatusCode,
			operationResult.StatusName,
			operationResult.StatusDescription)
		{
			IsApduCommandSuccessful = isApduCommandSuccessful;
		}
	}
}