namespace Lando.LowLevel.ResultsTypes
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

		public static OperationResult Successful
		{
			get
			{
				return ReturnCodeManager.GetErrorMessage(WinscardWrapper.SCARD_S_SUCCESS);
			}
		}
	}
}