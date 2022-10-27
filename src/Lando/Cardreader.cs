using System;
using System.Diagnostics;
using Lando.Actions;
using Lando.LowLevel;
using Lando.Watcher;
using Lando.Extensions;

namespace Lando
{
	/// <summary>
	/// Provides hardware reader events.
	/// </summary>
	public class Cardreader : IDisposable
	{
		private static readonly TraceSource Logger = new TraceSource("Lando", SourceLevels.All);

		internal readonly LowLevelCardReader LowlevelReader;
		internal readonly Watcher.Watcher Reader;
		internal readonly ActionQueue ActionQueue;

		private bool _isCardreaderWatchStarted;

		private bool _disposed = false;

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
			ActionQueue = new ActionQueue(LowlevelReader);

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
			if (_disposed) throw new ObjectDisposedException("Cardreader", "Cannot access a disposed object.");

			if (_isCardreaderWatchStarted)
				return;

			ActionQueue.Start();
			Reader.Start();

			_isCardreaderWatchStarted = true;
		}

		/// <summary>
		/// Stops reader listening.
		/// </summary>
		public void StopWatch()
		{
			if (_disposed) throw new ObjectDisposedException("Cardreader", "Cannot access a disposed object.");

			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: Stopping");
			Logger.Flush();

			ActionQueue.Stop();
			Reader.Stop();
		}

		public void UpdateLedAndBuzzer(ContactlessCard card, LedBuzzerStatus status)
		{
			if (_disposed) throw new ObjectDisposedException("Cardreader", "Cannot access a disposed object.");
			if (card == null) throw new ArgumentNullException("card");

			ActionQueue.EnqueueAction(new UpdateLedAndBuzzerAction(card, status));
		}

		public void SetBuzzerOutputForCardDetection(ContactlessCard card, bool shouldBuzzWhenCardDetected)
		{
			if (_disposed) throw new ObjectDisposedException("Cardreader", "Cannot access a disposed object.");
			if (card == null) throw new ArgumentNullException("card");

			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: SetBuzzerOutputForCardDetection entering");
			Logger.Flush();

			ActionQueue.EnqueueAction(new SetBuzzerOutputForCardDetectionAction(card, shouldBuzzWhenCardDetected));

			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: SetBuzzerOutputForCardDetection done");
			Logger.Flush();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (Reader != null)
					{
						Reader.Stop();
					}

					if (LowlevelReader != null)
					{
						LowlevelReader.Dispose();
					}
				}

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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
			var card = new ContactlessCard(e.Card);

			Logger.TraceEvent(TraceEventType.Verbose, 0, "Cardreader: Save invocation of CardDisconnected");
			Logger.Flush();

			CardDisconnected.SafeInvoke(this, new CardreaderEventArgs(card, e.Card.CardreaderName);
		}
	}
}