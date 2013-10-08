using System;
using System.Diagnostics;
using Lando.LowLevel;
using Lando.Watcher;
using Lando.Extensions;

namespace Lando
{
	/// <summary>
	/// Provides hardware reader events.
	/// </summary>
	public class Cardreader
	{
		private static readonly TraceSource Logger = new TraceSource("Lando", SourceLevels.All);

		internal readonly LowLevelCardReader LowlevelReader;
		internal readonly Watcher.Watcher Reader;

		private bool _isCardreaderWatchStarted;

		/// <summary>
		/// Occurs when connection with cardreader established.
		/// </summary>
		public virtual event CardreaderEventHandler CardreaderConnected;
		/// <summary>
		/// Occurs when connection with cardreader lost.
		/// </summary>
		public virtual event CardreaderEventHandler CardreaderDisconnected;
		/// <summary>
		/// Occurs when card connected.
		/// </summary>
		public virtual event CardreaderEventHandler CardConnected;
		/// <summary>
		/// Occurs when card disconnected.
		/// </summary>
		public virtual event CardreaderEventHandler CardDisconnected;

		public Cardreader()
		{
			LowlevelReader = new LowLevelCardReader();
			Reader = new Watcher.Watcher(LowlevelReader);

			Reader.CardConnected += OnCardConnected;
			Reader.CardDisconnected += OnCardDisconnected;
			Reader.CardreaderConnected += OnCardreaderConnected;
			Reader.CardreaderDisconnected += OnCardreaderDisconnected;
		}

		/// <summary>
		/// Starts reader listening.
		/// </summary>
		public void StartWatch()
		{
			if (_isCardreaderWatchStarted)
				return;

			Reader.Start();

			_isCardreaderWatchStarted = true;
		}

		/// <summary>
		/// Stops reader listening.
		/// </summary>
		public void StopWatch()
		{
			Reader.Stop();
		}

		public void UpdateLedAndBuzzer(ContactlessCard card, LedBuzzerStatus status)
		{
			if (card == null) throw new ArgumentNullException("card");

			LowlevelReader.UpdateLedAndBuzzer(
				card.Card, status.GetLedState(),
				status.GetT1(), status.GetT2(),
				status.GetRepetition(),
				status.GetBuzzerLink());
		}

		public void SetBuzzerOutputForCardDetection(ContactlessCard card, bool shouldBuzzWhenCardDetected)
		{
			if (card == null) throw new ArgumentNullException("card");

			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: SetBuzzerOutputForCardDetection entering");
			Logger.Flush();

			LowlevelReader.SetBuzzerOutputForCardDetection(card.Card, shouldBuzzWhenCardDetected);

			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: SetBuzzerOutputForCardDetection done");
			Logger.Flush();
		}

		internal virtual void OnCardreaderConnected(object sender, WatcherCardreaderEventArgs e)
		{
			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: Save invocation of CardreaderConnected");
			Logger.Flush();

			CardreaderConnected.SafeInvoke(this, new CardreaderEventArgs(e.CardreaderName));
		}

		internal virtual void OnCardreaderDisconnected(object sender, WatcherCardreaderEventArgs e)
		{
			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: Save invocation of CardreaderDisconnected");
			Logger.Flush();

			CardreaderDisconnected.SafeInvoke(this, new CardreaderEventArgs(e.CardreaderName));
		}

		internal virtual void OnCardConnected(object sender, WatcherCardEventArgs e)
		{
			var card = new ContactlessCard(e.Card);

			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: Save invocation of CardConnected");
			Logger.Flush();

			CardConnected.SafeInvoke(this, new CardreaderEventArgs(card, e.Card.CardreaderName));
		}

		internal virtual void OnCardDisconnected(object sender, WatcherCardEventArgs e)
		{
			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: Save invocation of CardDisconnected");
			Logger.Flush();

			CardDisconnected.SafeInvoke(this, new CardreaderEventArgs((string)null));
		}
	}
}