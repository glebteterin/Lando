namespace Lando.LowLevel.ResultsTypes
{
	public class ApduOperationResult : OperationResult
	{
		public bool IsApduCommandSuccessful { get; set; }

		public ApduOperationResult(OperationResult operationResult)
			: base(
			operationResult.IsSuccessful,
			operationResult.StatusCode,
			operationResult.StatusName,
			operationResult.StatusDescription)
		{
		}
	}
}