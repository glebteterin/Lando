using Lando.LowLevel.ResultsTypes;

namespace Lando.LowLevel
{
	internal static class ReturnCodeManager
	{
		public static ApduOperationResult IsApduSuccessful(ApduResponse response)
		{
			var baseOperationResult = GetErrorMessage(response.ReturnCode);
			var apduOperationStatus = Check90_00(response.RecvBuff, response.ResponseLength);

			return new ApduOperationResult(baseOperationResult, apduOperationStatus);
		}

		/// <see cref="http://msdn.microsoft.com/en-us/library/ms936965.aspx"/>
		/// <summary>
		/// Returns description of error code.
		/// </summary>
		public static OperationResult GetErrorMessage(int code)
		{
			if (code == WinscardWrapper.SCARD_S_SUCCESS)
				return new OperationResult(true, code, "SCARD_S_SUCCESS", " No error was encountered.");

			string codeName;
			string codeDescription;

			switch (code)
			{
				case WinscardWrapper.SCARD_E_CANCELLED:
					codeName = "The action was canceled by an SCardCancel request.";
					codeDescription = "SCARD_E_CANCELLED";
					break;
				case WinscardWrapper.SCARD_E_CANT_DISPOSE:
					codeName = "The system could not dispose of the media in the requested manner.";
					codeDescription = "SCARD_E_CANT_DISPOSE";
					break;
				case WinscardWrapper.SCARD_E_CARD_UNSUPPORTED:
					codeName = "The smart card does not meet minimal requirements for support.";
					codeDescription = "SCARD_E_CARD_UNSUPPORTED";
					break;
				case WinscardWrapper.SCARD_E_DUPLICATE_READER:
					codeName = "The reader driver didn't produce a unique reader name.";
					codeDescription = "SCARD_E_DUPLICATE_READER";
					break;
				case WinscardWrapper.SCARD_E_INSUFFICIENT_BUFFER:
					codeName = "The data buffer for returned data is too small for the returned data.";
					codeDescription = "SCARD_E_INSUFFICIENT_BUFFER";
					break;
				case WinscardWrapper.SCARD_E_INVALID_ATR:
					codeName = "An ATR string obtained from the registry is not a valid ATR string.";
					codeDescription = "SCARD_E_INVALID_ATR";
					break;
				case WinscardWrapper.SCARD_E_INVALID_HANDLE:
					codeName = "The supplied handle was invalid.";
					codeDescription = "SCARD_E_INVALID_HANDLE";
					break;
				case WinscardWrapper.SCARD_E_INVALID_PARAMETER:
					codeName = "One or more of the supplied parameters could not be properly interpreted.";
					codeDescription = "SCARD_E_INVALID_PARAMETER";
					break;
				case WinscardWrapper.SCARD_E_INVALID_TARGET:
					codeName = "Registry startup information is missing or invalid.";
					codeDescription = "SCARD_E_INVALID_TARGET";
					break;
				case WinscardWrapper.SCARD_E_INVALID_VALUE:
					codeName = "One or more of the supplied parameter values could not be properly interpreted.";
					codeDescription = "SCARD_E_INVALID_VALUE";
					break;
				case WinscardWrapper.SCARD_E_NOT_READY:
					codeName = "The reader or card is not ready to accept commands.";
					codeDescription = "SCARD_E_NOT_READY";
					break;
				case WinscardWrapper.SCARD_E_NOT_TRANSACTED:
					codeName = "An attempt was made to end a non-existent transaction.";
					codeDescription = "SCARD_E_NOT_TRANSACTED";
					break;
				case WinscardWrapper.SCARD_E_NO_MEMORY:
					codeName = "Not enough memory available to complete this command.";
					codeDescription = "SCARD_E_NO_MEMORY";
					break;
				case WinscardWrapper.SCARD_E_NO_SERVICE:
					codeName = "The smart card resource manager is not running.";
					codeDescription = "SCARD_E_NO_SERVICE";
					break;
				case WinscardWrapper.SCARD_E_NO_SMARTCARD:
					codeName = "The operation requires a smart card, but no smart card is currently in the device.";
					codeDescription = "SCARD_E_NO_SMARTCARD";
					break;
				case WinscardWrapper.SCARD_E_PCI_TOO_SMALL:
					codeName = "The PCI receive buffer was too small.";
					codeDescription = "SCARD_E_PCI_TOO_SMALL";
					break;
				case WinscardWrapper.SCARD_E_PROTO_MISMATCH:
					codeName = "The requested protocols are incompatible with the protocol currently in use with the card.";
					codeDescription = "SCARD_E_PROTO_MISMATCH";
					break;
				case WinscardWrapper.SCARD_E_READER_UNAVAILABLE:
					codeName = "The specified reader is not currently available for use.";
					codeDescription = "SCARD_E_READER_UNAVAILABLE";
					break;
				case WinscardWrapper.SCARD_E_READER_UNSUPPORTED:
					codeName = "The reader driver does not meet minimal requirements for support.";
					codeDescription = "SCARD_E_READER_UNSUPPORTED";
					break;
				case WinscardWrapper.SCARD_E_SERVICE_STOPPED:
					codeName = "The smart card resource manager has shut down.";
					codeDescription = "SCARD_E_SERVICE_STOPPED";
					break;
				case WinscardWrapper.SCARD_E_SHARING_VIOLATION:
					codeName = "The smart card cannot be accessed because of other outstanding connections.";
					codeDescription = "SCARD_E_SHARING_VIOLATION";
					break;
				case WinscardWrapper.SCARD_E_SYSTEM_CANCELLED:
					codeName = "The action was canceled by the system, presumably to log off or shut down.";
					codeDescription = "SCARD_E_SYSTEM_CANCELLED";
					break;
				case WinscardWrapper.SCARD_E_TIMEOUT:
					codeName = "The user-specified timeout value has expired.";
					codeDescription = "SCARD_E_TIMEOUT";
					break;
				case WinscardWrapper.SCARD_E_UNKNOWN_CARD:
					codeName = "The specified smart card name is not recognized.";
					codeDescription = "SCARD_E_UNKNOWN_CARD";
					break;
				case WinscardWrapper.SCARD_E_UNKNOWN_READER:
					codeName = "The specified reader name is not recognized.";
					codeDescription = "SCARD_E_UNKNOWN_READER";
					break;
				case WinscardWrapper.SCARD_F_COMM_ERROR:
					codeName = "An internal communications error has been detected.";
					codeDescription = "SCARD_F_COMM_ERROR";
					break;
				case WinscardWrapper.SCARD_F_INTERNAL_ERROR:
					codeName = "An internal consistency check failed.";
					codeDescription = "SCARD_F_INTERNAL_ERROR";
					break;
				case WinscardWrapper.SCARD_F_UNKNOWN_ERROR:
					codeName = "An internal error has been detected, but the source is unknown.";
					codeDescription = "SCARD_F_UNKNOWN_ERROR";
					break;
				case WinscardWrapper.SCARD_F_WAITED_TOO_LONG:
					codeName = "An internal consistency timer has expired.";
					codeDescription = "SCARD_F_WAITED_TOO_LONG";
					break;
				case WinscardWrapper.SCARD_W_REMOVED_CARD:
					codeName = "The smart card has been removed, so that further communication is not possible.";
					codeDescription = "SCARD_W_REMOVED_CARD";
					break;
				case WinscardWrapper.SCARD_W_RESET_CARD:
					codeName = "The smart card has been reset, so any shared state information is invalid.";
					codeDescription = "SCARD_W_RESET_CARD";
					break;
				case WinscardWrapper.SCARD_W_RESET_CARD2:
					codeName = "The smart card has been reset, so any shared state information is invalid.";
					codeDescription = "SCARD_W_RESET_CARD2";
					break;
				case WinscardWrapper.SCARD_W_UNPOWERED_CARD:
					codeName = "Power has been removed from the smart card, so that further communication is not possible.";
					codeDescription = "SCARD_W_UNPOWERED_CARD";
					break;
				case WinscardWrapper.SCARD_W_UNRESPONSIVE_CARD:
					codeName = "The smart card is not responding to a reset.";
					codeDescription = "SCARD_W_UNRESPONSIVE_CARD";
					break;
				case WinscardWrapper.SCARD_W_UNSUPPORTED_CARD:
					codeName = "The reader cannot communicate with the card, due to ATR string configuration conflicts.";
					codeDescription = "SCARD_W_UNSUPPORTED_CARD";
					break;

				case WinscardWrapper.SCARD_E_NO_READERS_AVAILABLE:
					codeName = "No smart card reader is available.";
					codeDescription = "SCARD_E_NO_READERS_AVAILABLE";
					break;

				case WinscardWrapper.ERROR_INVALID_HANDLE:
					codeName = "The handle that was passed to the API has been either invalidated or closed.";
					codeDescription = "ERROR_INVALID_HANDLE";
					break;

				case WinscardWrapper.ERROR_OPERATION_ABORTED:
					codeName = "The I/O operation has been aborted because of either a thread exit or an application request.";
					codeDescription = "ERROR_OPERATION_ABORTED";
					break;

				default:
					codeName = "UNKNOWN";
					codeDescription = "";
					break;
			}

			return new OperationResult(false, code, codeName, codeDescription);
		}

		private static bool Check90_00(byte[] receiveBytes, int fullResponseLength)
		{
			var firstByte = fullResponseLength - 2;
			var secondByte = firstByte + 1;

			if (firstByte <= 0 || firstByte >= receiveBytes.Length || secondByte >= receiveBytes.Length)
				return false;

			var result = false;
			if (receiveBytes[firstByte] == 144 && receiveBytes[secondByte] == 0) //90 00
				result = true;

			return result;
		}
	}
}