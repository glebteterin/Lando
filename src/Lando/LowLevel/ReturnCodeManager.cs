using Lando.LowLevel.ResultsTypes;

namespace Lando.LowLevel
{
	internal static class ReturnCodeManager
	{
		public static bool GetCardState(int returnCode)
		{
			switch (returnCode)
			{
				case WinscardWrapper.SCARD_S_SUCCESS: return true;
				default: return false;
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