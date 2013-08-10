namespace Lando.Watcher
{
	internal class WatcherCardreaderEventArgs
	{
		public string CardreaderName { get; set; }

		public WatcherCardreaderEventArgs(string cardreaderName)
		{
			CardreaderName = cardreaderName;
		}
	}
}