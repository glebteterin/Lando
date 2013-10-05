using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lando.LowLevel;
using Lando.LowLevel.ResultsTypes;

namespace Lando.Watcher
{
	internal class Watcher
	{
		private static readonly TraceSource Logger = new TraceSource("Lando", SourceLevels.All);

		private const string PnpNotification = "\\\\?PnP?\\Notification";

		private static readonly AsyncOperation AsyncOperation = AsyncOperationManager.CreateOperation(null);
		private readonly ConcurrentDictionary<string, Card> _attachedCardStatuses = new ConcurrentDictionary<string, Card>();
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
				_cardreader.ReleaseAllContexts();
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
			_workingThread.IsBackground = true;
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
						Logger.TraceEvent(TraceEventType.Information, 0, "WaitForChanges operation result is " + operationResult.StatusName);
						Logger.Flush();

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
							!DidCardreaderHaveConnectedCard(cardreaderStatus.Name) &&
							!newStatuses.Contains(CardreaderStatus.StatusType.CardUnresponsive))
						{
							var connectResult = _cardreader.Connect(cardreaderStatus.Name);

							if (connectResult.IsSuccessful)
							{
								var connectedLowlevelCard = connectResult.ConnectedCard;

								var result = _cardreader.GetCardId(connectedLowlevelCard);

								if (result.IsCompletelySuccessful)
								{
									if (!DidCardreaderHaveConnectedCard(cardreaderStatus.Name))
									{
										connectedLowlevelCard.IdBytes = result.Bytes;

										RememberCardreaderHadCard(cardreaderStatus.Name, connectedLowlevelCard);

										RaiseCardConnectedEvent(connectedLowlevelCard);
									}
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

							if (DidCardreaderHaveConnectedCard(cardreaderStatus.Name))
							{
								var currentCard = LastConnectedCard(cardreaderStatus.Name);
								if (currentCard != null)
									_cardreader.DisconnectCard(currentCard);

								// reset previously connected card and raise the card connected event
								ForgotAboutCardreaderHadCard(cardreaderStatus.Name);
								RaiseCardDisconnectedEvent();
							}
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
			Logger.TraceEvent(TraceEventType.Information, 0, "Raising CardConnected event");
			Logger.Flush();

			SendOrPostCallback cb = state => CardConnected(null, new WatcherCardEventArgs(connectedLowlevelCard));
			AsyncOperation.Post(cb, null);
		}

		private void RaiseCardDisconnectedEvent()
		{
			Logger.TraceEvent(TraceEventType.Information, 0, "Raising CardDisconnected event");
			Logger.Flush();

			SendOrPostCallback cb = state => CardDisconnected(null, new WatcherCardEventArgs());
			AsyncOperation.Post(cb, null);
		}

		private void RaiseCardreaderConnectedEvent(string readerName)
		{
			Logger.TraceEvent(TraceEventType.Information, 0, "Raising CardreaderConnected event");
			Logger.Flush();

			SendOrPostCallback cb = state => CardreaderConnected(null, new WatcherCardreaderEventArgs(readerName));
			AsyncOperation.Post(cb, null);
		}

		private void RaiseCardreaderDisconnectedEvent(CardreaderStatus cardreaderStatus)
		{
			Logger.TraceEvent(TraceEventType.Information, 0, "Raising CardreaderDisconnected event");
			Logger.Flush();

			SendOrPostCallback cb =
				state => CardreaderDisconnected(null, new WatcherCardreaderEventArgs(cardreaderStatus.Name));
			AsyncOperation.Post(cb, null);
		}

		private bool DidCardreaderHaveConnectedCard(string cardreaderName)
		{
			Card attachedCard = null;

			_attachedCardStatuses.TryGetValue(cardreaderName, out attachedCard);

			var result = attachedCard != null;

			Logger.TraceEvent(TraceEventType.Information, 0, "Checking for a previously connected card. Result: " + result);
			Logger.Flush();

			return result;
		}

		private Card LastConnectedCard(string cardreaderName)
		{
			Card attachedCard = null;

			_attachedCardStatuses.TryGetValue(cardreaderName, out attachedCard);

			Logger.TraceEvent(TraceEventType.Information, 0, "Checking for a previously connected card. Result: " + (attachedCard != null ? "card exist" : "card not exist"));
			Logger.Flush();

			return attachedCard;
		}

		private void RememberCardreaderHadCard(string readerName, Card connectedCard)
		{
			Logger.TraceEvent(TraceEventType.Information, 0, "Marking that cardreader ({0}) had a card", readerName);
			Logger.Flush();

			_attachedCardStatuses.AddOrUpdate(readerName, connectedCard, (key, newValue) => connectedCard);
		}

		private void ForgotAboutCardreaderHadCard(string readerName)
		{
			Logger.TraceEvent(TraceEventType.Information, 0, "Forgetting that the previously connected card of {0}", readerName);
			Logger.Flush();

			Card tmp;
			_attachedCardStatuses.TryRemove(readerName, out tmp);
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
				Logger.TraceEvent(TraceEventType.Information, 0, "New statuses for {0}:", cardreaderStatus.Name);
				foreach (var statusType in cardreaderStatus.Statuses)
				{
					Logger.TraceEvent(TraceEventType.Information, 0, "Status: {0}", statusType);
				}
			}
			Logger.Flush();
		}
	}
}