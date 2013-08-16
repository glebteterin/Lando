using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Lando.LowLevel;
using Lando.LowLevel.ResultsTypes;
using NLog;

namespace Lando.Watcher
{
	internal class Watcher
	{
		private static readonly Logger Logger = LogManager.GetLogger("LandoLog");

		private const string PnpNotification = "\\\\?PnP?\\Notification";

		private static readonly AsyncOperation AsyncOperation = AsyncOperationManager.CreateOperation(null);
		private readonly List<CardreaderStatus> _statuses = new List<CardreaderStatus>();
		private readonly LowLevelCardReader _cardreader;

		private int _cardreadersNumber;
		private bool _started;
		private Thread _workingThread;

		public event WatcherCardEventHandler CardConnected;
		public event WatcherCardEventHandler CardDisconnected;
		public event WatcherCardreaderEventHandler CardreaderConnected;
		public event WatcherCardreaderEventHandler CardreaderDisconnected;

		public Watcher(LowLevelCardReader cardreader)
		{
			_cardreader = cardreader;
		}

		public void Start()
		{
			EstablishContext();

			DetectAvailableCardreaders();

			StartWatch();
		}

		public void Stop()
		{
			if (_started)
			{
				_started = false;
				_cardreader.ReleaseContext();
			}
		}

		private void EstablishContext()
		{
			var operationResult = _cardreader.EstablishContext();

			if (!operationResult.IsSuccessful)
				throw new SmartCardException(operationResult);
		}

		private void DetectAvailableCardreaders()
		{
			string[] readerNames = null;

			var operationResultType = _cardreader.GetCardReadersList(out readerNames);

			if (operationResultType.IsSuccessful)
			{
				foreach (var readerName in readerNames)
				{
					if (_statuses.All(x => x.Name != readerName))
					{
						_statuses.Add(new CardreaderStatus(readerName));

						_cardreadersNumber++;

						RaiseCardreaderConnectedEvent(readerName);
					}
				}
			}
		}

		private void StartWatch()
		{
			_started = true;

			_workingThread = new Thread(Watch);
			_workingThread.Start();
		}

		private void Watch()
		{
			while (_started)
			{
				var statuses = new List<CardreaderStatus>(_statuses);

				if (statuses.All(x => x.Name != PnpNotification))
				{
					statuses.Add(new CardreaderPnpStatus(_cardreadersNumber));
				}

				var statusesParam = statuses.ToArray();

				var operationResult = _cardreader.WaitForChanges(ref statusesParam);

				if (!operationResult.IsSuccessful)
				{
					// when we call thread.abort the WaitForChanges returns Failed
					// so check for that
					if (_started)
					{
						Logger.Error("WaitForChanges operation result is " + operationResult.StatusName);

						throw new SmartCardException(operationResult);
					}
				}

				Log(statusesParam);

				foreach (var cardreaderStatus in statusesParam)
				{
					if (!cardreaderStatus.IsChanged)
						continue;

					var newStatuses = cardreaderStatus.Statuses;

					if (newStatuses.Contains(CardreaderStatus.StatusType.Changed))
					{
						// first call after a card was putted on a cardreader will contain two statuses:
						// Changed
						// CardConnected

						// and right after that there will be a second call with statuses:
						// Changed
						// CardConnected
						// CardInUse

						// so I filter out the second call

						// CardUnresponsive status means that card was removed too fast

						if (newStatuses.Contains(CardreaderStatus.StatusType.CardConnected) &&
							!newStatuses.Contains(CardreaderStatus.StatusType.CardInUse) &&
							!newStatuses.Contains(CardreaderStatus.StatusType.CardUnresponsive))
						{
							var connectResult = _cardreader.Connect(cardreaderStatus.Name);

							if (connectResult.IsSuccessful)
							{
								var connectedLowlevelCard = connectResult.ConnectedCard;

								var result = _cardreader.GetCardId(connectedLowlevelCard);

								if (result.IsCompletelySuccessful)
								{
									connectedLowlevelCard.IdBytes = result.Bytes;

									RaiseCardConnectedEvent(connectedLowlevelCard);
								}
								else
								{
									if (IsNoCardreaderResult(connectResult))
									{
										_cardreadersNumber--;

										RaiseCardreaderDisconnectedEvent(cardreaderStatus);

										RemoveCardreaderFromList(cardreaderStatus.Name);
									}
									else
									{
										// if card was detached before than id was received
										// reset updated status
										cardreaderStatus.NewStatusFlags = cardreaderStatus.CurrentStatusFlags;
									}
								}
							}
							else if (IsNoCardreaderResult(connectResult))
							{
								_cardreadersNumber--;

								RaiseCardreaderDisconnectedEvent(cardreaderStatus);

								RemoveCardreaderFromList(cardreaderStatus.Name);
							}
							else if (connectResult.StatusCode == WinscardWrapper.SCARD_E_NO_SMARTCARD)
							{
								// card was detached too fast
								// don't react to this
							}
							else
							{
								throw new SmartCardException(connectResult);
							}
						}
						if (newStatuses.Contains(CardreaderStatus.StatusType.CardDisconnected) &&
							!newStatuses.Contains(CardreaderStatus.StatusType.CardUnresponsive) &&
							cardreaderStatus.CurrentStatusFlags != 0)
						{
							// don't want to raise disconnected event for unresponsive card
							// because it means that card wasn't connected properly

							// cardreaderStatus.CurrentStatusFlags != 0
							// means that this is not a new cardreader status and it has some previous status
							// cause otherways it doesn't make sense
							RaiseCardDisconnectedEvent();
						}
						if (newStatuses.Contains(CardreaderStatus.StatusType.CardreaderDisconnected))
						{
							_cardreadersNumber--;

							RaiseCardreaderDisconnectedEvent(cardreaderStatus);

							RemoveCardreaderFromList(cardreaderStatus.Name);
						}
						if (cardreaderStatus.Name == PnpNotification)
						{
							DetectAvailableCardreaders();
						}

						cardreaderStatus.Swap();
					}
				}
			}
		}

		private void RaiseCardConnectedEvent(Card connectedLowlevelCard)
		{
			SendOrPostCallback cb = state => CardConnected(null, new WatcherCardEventArgs(connectedLowlevelCard));
			AsyncOperation.Post(cb, null);
		}

		private void RaiseCardDisconnectedEvent()
		{
			SendOrPostCallback cb = state => CardDisconnected(null, new WatcherCardEventArgs());
			AsyncOperation.Post(cb, null);
		}

		private void RaiseCardreaderConnectedEvent(string readerName)
		{
			SendOrPostCallback cb = state => CardreaderConnected(null, new WatcherCardreaderEventArgs(readerName));
			AsyncOperation.Post(cb, null);
		}

		private void RaiseCardreaderDisconnectedEvent(CardreaderStatus cardreaderStatus)
		{
			SendOrPostCallback cb =
				state => CardreaderDisconnected(null, new WatcherCardreaderEventArgs(cardreaderStatus.Name));
			AsyncOperation.Post(cb, null);
		}

		private void RemoveCardreaderFromList(string cardreaderName)
		{
			while (_statuses.Any(x => x.Name == cardreaderName))
				_statuses.Remove(_statuses.First(x => x.Name == cardreaderName));
		}

		private bool IsNoCardreaderResult(OperationResult operationResult)
		{
			return operationResult.StatusCode == WinscardWrapper.SCARD_E_UNKNOWN_READER ||
					operationResult.StatusCode == WinscardWrapper.SCARD_E_INVALID_HANDLE ||
					operationResult.StatusCode == WinscardWrapper.SCARD_E_READER_UNAVAILABLE ||
					operationResult.StatusCode == WinscardWrapper.SCARD_E_UNKNOWN_READER;
		}

		private void Log(IEnumerable<CardreaderStatus> statuses)
		{
			foreach (var cardreaderStatus in statuses)
			{
				Logger.Trace("New statuses for {0}:", cardreaderStatus.Name);
				foreach (var statusType in cardreaderStatus.Statuses)
				{
					Logger.Trace("Status: {0}", statusType);
				}
			}
		}
	}
}