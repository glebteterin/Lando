using System;

namespace Lando.LowLevel
{
	internal class SmartCardException : Exception
	{
		public int ErrorCode { get; private set; }
		public OperationResult OperationResult { get; private set; }

		public override string Message
		{
			get
			{
				return string.Format("Unexpected operation result. {0}({1}) - {2}",
					OperationResult.StatusName, OperationResult.StatusCode, OperationResult.StatusDescription);
			}
		}

		public SmartCardException(int errorCode)
		{
			ErrorCode = errorCode;
		}

		public SmartCardException(OperationResult operationResult)
		{
			OperationResult = operationResult;
		}
	}
}