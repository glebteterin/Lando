using System;
using Lando.LowLevel.ResultsTypes;

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

		public SmartCardException(OperationResult operationResult)
		{
			OperationResult = operationResult;
		}
	}
}