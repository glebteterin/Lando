namespace Lando.LowLevel.ResultsTypes
{
	internal class ConnectResult : OperationResult
	{
		public Card ConnectedCard { get; set; }

		public ConnectResult(OperationResult operationResult) :
			base(
			operationResult.IsSuccessful,
			operationResult.StatusCode,
			operationResult.StatusName,
			operationResult.StatusDescription)
		{
		}
	}
}