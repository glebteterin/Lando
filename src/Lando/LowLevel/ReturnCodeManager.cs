using Lando.LowLevel.Enums;
using Lando.LowLevel.ResultsTypes;

namespace Lando.LowLevel
{
	internal static class ReturnCodeManager
	{
		public static OperationResultType GetCardState(int returnCode)
		{
			switch (returnCode)
			{
				case WinscardWrapper.SCARD_S_SUCCESS: return OperationResultType.Success;
				case WinscardWrapper.ERROR_INVALID_HANDLE: return OperationResultType.InvalidHandle;
				case WinscardWrapper.SCARD_W_REMOVED_CARD: return OperationResultType.CardRemoved;
				case WinscardWrapper.SCARD_E_READER_UNAVAILABLE: return OperationResultType.ReaderUnavailable;
				case WinscardWrapper.SCARD_W_RESET_CARD: return OperationResultType.CardResetted;
				case WinscardWrapper.SCARD_W_RESET_CARD2: return OperationResultType.CardResetted;
				case WinscardWrapper.ERROR_OPERATION_ABORTED: return OperationResultType.SystemError;
				default: throw new SmartCardException(returnCode);
			}
		}

		public static OperationResultType DisconnectCard(int returnCode)
		{
			switch (returnCode)
			{
				case WinscardWrapper.SCARD_S_SUCCESS: return OperationResultType.Success;
				default: throw new SmartCardException(returnCode);
			}
		}

		public static bool IsApduSuccessful(SendApduResult sendResult)
		{
			return Check90_00(sendResult.RecvBuff, sendResult.ResponseLength);
		}

		private static bool Check90_00(byte[] receiveBytes, int fullResponseLength)
		{
			var firstIndex = fullResponseLength - 2;

			var result = false;
			if (receiveBytes[firstIndex] == 144 && receiveBytes[firstIndex + 1] == 0) //90 00
				result = true;

			return result;
		}
	}
}