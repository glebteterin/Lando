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
	}
}