using Lando.LowLevel;

namespace Lando.Watcher
{
	internal class WatcherCardEventArgs
	{
		public Card Card { get; set; }

		public WatcherCardEventArgs()
		{
		}

		public WatcherCardEventArgs(Card card)
		{
			Card = card;
		}
	}
}