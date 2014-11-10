using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Lando.LowLevel;

namespace Lando
{
	internal class ActionQueue : IDisposable
	{
		private static readonly TraceSource Logger = new TraceSource("Lando", SourceLevels.All);

		private readonly BlockingCollection<ICardreaderAction> _actionQueue = new BlockingCollection<ICardreaderAction>();
		private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

		private readonly LowLevelCardReader _lowLevelCardReader;

		private Thread _workingThread;
		private bool _started;

		private bool _disposed = false;

		public ActionQueue(LowLevelCardReader lowLevelCardReader)
		{
			_lowLevelCardReader = lowLevelCardReader;
		}

		public void Start()
		{
			if (_disposed) throw new ObjectDisposedException("ActionQueue", "Cannot access a disposed object.");

			_started = true;

			_workingThread = new Thread(ProcessQueue);
			_workingThread.IsBackground = true;
			_workingThread.Start(_tokenSource.Token);
		}

		public void Stop()
		{
			if (_started)
			{
				Logger.TraceEvent(TraceEventType.Verbose, 0, "ActionQueue: Stopping");
				Logger.Flush();

				_started = false;
				_tokenSource.Cancel();
			}
		}

		public void EnqueueAction(ICardreaderAction action)
		{
			if (_disposed) throw new ObjectDisposedException("ActionQueue", "Cannot enqueue to a disposed queue.");

			_actionQueue.Add(action);
		}

		private void ProcessQueue(object cancellationTokenObject)
		{
			var token = (CancellationToken) cancellationTokenObject;

			while (_started)
			{
				ICardreaderAction action = null;

				try
				{
					action = _actionQueue.Take(token);
				}
				catch (OperationCanceledException)
				{
					// Take operation was canceled
					Logger.TraceEvent(TraceEventType.Verbose, 0, "Token was canceled for Action Queue");
					Logger.Flush();
				}

				if (action == null)
					continue;

				action.Execute(_lowLevelCardReader);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Stop();

					if (_tokenSource != null)
					{
						_tokenSource.Cancel();
						_tokenSource.Dispose();
					}

					if (_actionQueue != null)
					{
						_actionQueue.Dispose();
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
	}

	internal interface ICardreaderAction
	{
		CardreaderActionType Type { get; }

		void Execute(LowLevelCardReader lowLevelCardReader);
	}

	internal enum CardreaderActionType
	{
		UpdateLedAndBuzzer,
		SetBuzzerOutputForCardDetection
	}
}