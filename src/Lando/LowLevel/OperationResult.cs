namespace Lando.LowLevel
{
	public class OperationResult
	{
		public bool IsSuccessful { get; private set; }

		public int StatusCode { get; private set; }

		public string StatusName { get; set; }

		public string StatusDescription { get; private set; }

		public OperationResult(bool isSuccessful, int statusCode, string statusName, string statusDescription)
		{
			IsSuccessful = isSuccessful;
			StatusCode = statusCode;
			StatusName = statusName;
			StatusDescription = statusDescription;
		}
	}
}