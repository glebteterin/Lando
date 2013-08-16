namespace Lando.Extensions
{
	public static class EventHandlerExtensions
	{
		public static void SafeInvoke(this CardreaderEventHandler evt, object sender, CardreaderEventArgs e)
		{
			if (evt != null)
			{
				evt(sender, e);
			}
		}
	}
}