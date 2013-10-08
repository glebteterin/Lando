using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lando.LowLevel.ResultsTypes;

namespace Lando.LowLevel
{
	internal class LowLevelCardReader : IDisposable
	{
		private static readonly TraceSource Logger = new TraceSource("Lando", SourceLevels.All);

		private static readonly int PciLength = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WinscardWrapper.SCARD_IO_REQUEST));

		private readonly ContextManager _contextManager = new ContextManager();
		private readonly CardConnectionManager _cardConnectionManager = new CardConnectionManager();

		private readonly object _locker = new object();

		private bool _disposed;

		/// <summary>
		/// Establish Context of Resource Manager.
		/// </summary>
		public OperationResult EstablishContext()
		{
			lock (_locker)
			{
				if (_contextManager.IsContextExist(Thread.CurrentThread.ManagedThreadId))
				{
					Logger.TraceEvent(TraceEventType.Verbose, 0, "Context is already established.");
					Logger.Flush();

					return new OperationResult(true, WinscardWrapper.SCARD_S_SUCCESS, null, null);
				}

				IntPtr resourceManagerContext;
				IntPtr notUsed1 = IntPtr.Zero;
				IntPtr notUsed2 = IntPtr.Zero;

				int returnCode = WinscardWrapper.SCardEstablishContext(WinscardWrapper.SCARD_SCOPE_USER,
																		notUsed1,
																		notUsed2,
																		out resourceManagerContext);

				if (returnCode == WinscardWrapper.SCARD_S_SUCCESS)
				{
					Logger.TraceEvent(TraceEventType.Verbose, 0, "Context established");
					Logger.Flush();

					_contextManager.AddContext(Thread.CurrentThread.ManagedThreadId, resourceManagerContext);
				}

				return ReturnCodeManager.GetErrorMessage(returnCode);
			}
		}

		public void ReleaseAllContexts()
		{
			lock (_locker)
			{
				var allThreads = _contextManager.GetAllThreads();

				foreach (var threadId in allThreads)
				{
					var threadContext = _contextManager.GetContext(threadId);

					var releaseResult = ReleaseContext(threadContext);

					if (releaseResult.IsSuccessful)
					{
						Logger.TraceEvent(TraceEventType.Verbose, 0, string.Format("Context released. Thread {0}", threadId));
						Logger.Flush();

						_contextManager.ContextReleased(threadId);
					}
				}
			}
		}

		private OperationResult ReleaseContext(IntPtr context)
		{
			int returnCode = WinscardWrapper.SCardReleaseContext(context);

			var operationResult = ReturnCodeManager.GetErrorMessage(returnCode);

			return operationResult;
		}

		/// <summary>
		/// Return card readers list.
		/// </summary>
		public OperationResult GetCardReadersList(out string[] readersList)
		{
			var resourceManagerContext = EstablishContextIfNotEstablished();

			readersList = new string[0];

			OperationResult result;
			int sizeOfReadersListStructure = 0;

			int returnCode = WinscardWrapper.SCardListReaders(resourceManagerContext, null, null, ref sizeOfReadersListStructure);

			if (returnCode != WinscardWrapper.SCARD_S_SUCCESS)
			{
				result = ReturnCodeManager.GetErrorMessage(returnCode);
			}
			else
			{
				// Fill reader list
				var cardReadersList = new byte[sizeOfReadersListStructure];
				returnCode = WinscardWrapper.SCardListReaders(resourceManagerContext, null, cardReadersList, ref sizeOfReadersListStructure);

				if (returnCode != WinscardWrapper.SCARD_S_SUCCESS)
				{
					result = ReturnCodeManager.GetErrorMessage(returnCode);
				}
				else
				{
					// Convert to strings
					readersList = ConvertReadersBuffer(cardReadersList);

					result = ReturnCodeManager.GetErrorMessage(returnCode);
				}
			}

			return result;
		}

		public OperationResult WaitForChanges(ref CardreaderStatus[] statuses)
		{
			var resourceManagerContext = EstablishContextIfNotEstablished();

			var scardStatuses = new WinscardWrapper.SCARD_READERSTATE[statuses.Length];

			for (var i = 0; i < statuses.Length; i++)
				scardStatuses[i] = statuses[i].ToScardStatus();

			Logger.TraceEvent(TraceEventType.Verbose, 0, "SCardGetStatusChange started");
			Logger.Flush();

			var returnCode = WinscardWrapper.SCardGetStatusChange(
				resourceManagerContext,
				WinscardWrapper.INFINITE,
				scardStatuses,
				scardStatuses.Length);

			Logger.TraceEvent(TraceEventType.Verbose, 0, "SCardGetStatusChange ended");
			Logger.Flush();

			var operationResult = ReturnCodeManager.GetErrorMessage(returnCode);

			if (operationResult.IsSuccessful)
			{
				for (var i = 0; i < statuses.Length; i++)
					statuses[i].NewStatusFlags = scardStatuses[i].dwEventState;
			}

			return operationResult;
		}

		/// <summary>
		/// Establishing a connection to smart card contained by a specific reader.
		/// <param name="cardreaderName">Card reader name to connection.</param>
		/// </summary>
		public ConnectResult Connect(string cardreaderName, Guid? idOverride = null)
		{
			var resourceManagerContext = EstablishContextIfNotEstablished();

			IntPtr cardConnectionHandle;
			int connectionProtocolType;

			int returnCode = WinscardWrapper.SCardConnect(
				resourceManagerContext,
				cardreaderName,
				WinscardWrapper.SCARD_SHARE_SHARED,
				WinscardWrapper.SCARD_PROTOCOL_T0 | WinscardWrapper.SCARD_PROTOCOL_T1,
				out cardConnectionHandle,
				out connectionProtocolType);

			var operationResult = ReturnCodeManager.GetErrorMessage(returnCode);
			var connectResult = new ConnectResult(operationResult);

			if (operationResult.IsSuccessful)
			{
				var newCard = new Card(
					Thread.CurrentThread.ManagedThreadId,
					cardConnectionHandle,
					cardreaderName,
					connectionProtocolType);

				_cardConnectionManager.AddConnection(
					idOverride ?? newCard.InternalUid,
					Thread.CurrentThread.ManagedThreadId,
					cardConnectionHandle);

				connectResult.ConnectedCard = newCard;
			}

			return connectResult;
		}

		/// <summary>
		/// The function provides the current status of a smart card in a reader.
		/// </summary>
		public OperationResult GetCardState(Card cardToRead)
		{
			if (cardToRead == null) throw new ArgumentNullException("cardToRead");

			var sizeOfReadersListStructure = 0;
			var cardStateStatus = 0;
			var dwActProtocol = 0;
			var tmpAtrBytes = new byte[257];
			var tmpAtrLen = 32;

			var returnCode = WinscardWrapper.SCardStatus(cardToRead.ConnectionHandle, cardToRead.CardreaderName, ref sizeOfReadersListStructure, ref cardStateStatus, ref dwActProtocol, ref tmpAtrBytes[0], ref tmpAtrLen);

			OperationResult result = ReturnCodeManager.GetErrorMessage(returnCode);

			if (result.IsSuccessful)
			{
				cardToRead.State = new CardState(cardStateStatus);
				cardToRead.Atr = tmpAtrBytes.Take(tmpAtrLen).ToArray();
				cardToRead.Protocol = dwActProtocol;
			}

			return result;
		}

		public ApduOperationResult UpdateLedAndBuzzer(Card card, byte ledState, byte t1, byte t2, byte repetitionNumber, byte buzzer)
		{
			var bytesToSend = new byte[9];
			bytesToSend[0] = 0xFF;
			bytesToSend[1] = 0x00;
			bytesToSend[2] = 0x40;
			bytesToSend[3] = ledState;
			bytesToSend[4] = 0x04;
			bytesToSend[5] = t1;
			bytesToSend[6] = t2;
			bytesToSend[7] = repetitionNumber;
			bytesToSend[8] = buzzer;

			const int responseCodeLength = 2;

			ApduResponse response = SendAPDU(card, bytesToSend, responseCodeLength);

			return ReturnCodeManager.IsApduSuccessful(response);
		}

		public ApduOperationResult SetBuzzerOutputForCardDetection(Card card, bool shouldBuzzWhenCardDetected)
		{
			var bytesToSend = new byte[5];
			bytesToSend[0] = 0xFF;
			bytesToSend[1] = 0x00;
			bytesToSend[2] = 0x52;
			bytesToSend[3] = shouldBuzzWhenCardDetected ? (byte)0xFF : (byte)0x00;
			bytesToSend[4] = 0x00;

			const int responseCodeLength = 2;

			ApduResponse response = SendAPDU(card, bytesToSend, responseCodeLength);

			return ReturnCodeManager.IsApduSuccessful(response);
		}

		/// <summary>
		/// Returns a card's UID.
		/// </summary>
		public ReceiveCardIdResult GetCardId(Card cardToRead)
		{
			if (cardToRead == null) throw new ArgumentNullException("cardToRead");

			var bytesToSend = new byte[5];
			bytesToSend[0] = 0xFF; // Class
			bytesToSend[1] = 0xCA; // INS
			bytesToSend[2] = 0x00; // P1
			bytesToSend[3] = 0x00; // P2
			bytesToSend[4] = 0x00; // LE:Full Length

			ApduResponse response = SendAPDU(cardToRead, bytesToSend, 10);

			const int responseCodeLength = 2;
			var responseLengthWithoutResponseCodes = response.ResponseLength - responseCodeLength;

			var receiveCardIdResult = new ReceiveCardIdResult(ReturnCodeManager.IsApduSuccessful(response));

			if (receiveCardIdResult.IsCompletelySuccessful)
			{
				//read UID bytes from apdu response
				receiveCardIdResult.Bytes = response.RecvBuff.Take(responseLengthWithoutResponseCodes).ToArray();
			}

			return receiveCardIdResult;
		}

		public OperationResult DisconnectCard(Card cardForDisconnect)
		{
			if (cardForDisconnect == null) throw new ArgumentNullException("cardForDisconnect");

			if (cardForDisconnect.ConnectionHandle != IntPtr.Zero)
			{
				int returnCode = WinscardWrapper.SCardDisconnect(cardForDisconnect.ConnectionHandle, WinscardWrapper.SCARD_UNPOWER_CARD);
				cardForDisconnect.ConnectionHandle = IntPtr.Zero;
				var result = ReturnCodeManager.GetErrorMessage(returnCode);

				if (result.IsSuccessful)
					_cardConnectionManager.CardDisconnected(cardForDisconnect.InternalUid);

				return result;
			}

			return OperationResult.Successful;
		}

		private ApduResponse SendAPDU(Card card, byte[] bytesToSend, int expectedRequestLength)
		{
			Logger.TraceEvent(TraceEventType.Verbose, 0, "SendAPDU started");
			Logger.TraceEvent(TraceEventType.Verbose, 0, "bytesToSend: {0}", BitConverter.ToString(bytesToSend));
			Logger.TraceEvent(TraceEventType.Verbose, 0, "card connection handle: {0}", card.ConnectionHandle);
			Logger.Flush();

			IntPtr cardConnectionHandle = card.ConnectionHandle;

			// establish a new temporary connection in case of context mismatch
			if (card.ThreadId != Thread.CurrentThread.ManagedThreadId)
			{
				Logger.TraceEvent(TraceEventType.Verbose, 0,
					string.Format("Card context mismatch. Original thread: {0}. Current thread: {1}",
					card.ThreadId,
					Thread.CurrentThread.ManagedThreadId));
				Logger.Flush();

				if (!_cardConnectionManager.IsConnectionExist(card.InternalUid, Thread.CurrentThread.ManagedThreadId))
				{
					// establish a new connection
					var connectionResult = Connect(card.CardreaderName, card.InternalUid);
					if (!connectionResult.IsSuccessful)
					{
						return new ApduResponse
									{
										ReturnCode = connectionResult.StatusCode,
										RecvBuff = new byte[0],
										ResponseLength = 0
									};
					}

					// use a handle of a new connection
					cardConnectionHandle = connectionResult.ConnectedCard.ConnectionHandle;

					Logger.TraceEvent(TraceEventType.Verbose, 0, "SendAPDU: new connection established. Handle: "
																	+ cardConnectionHandle);
				}
				else
				{
					cardConnectionHandle = _cardConnectionManager
						.GetConnection(card.InternalUid, Thread.CurrentThread.ManagedThreadId);

					Logger.TraceEvent(TraceEventType.Verbose, 0, "SendAPDU: existed card context found. Handle: "
																	+ cardConnectionHandle);
				}
			}

			var recvBuff = new byte[500];

			WinscardWrapper.SCARD_IO_REQUEST pioSendRequest;
			pioSendRequest.dwProtocol = card.Protocol;
			pioSendRequest.cbPciLength = PciLength;

			int returnCode = WinscardWrapper.SCardTransmit(
				cardConnectionHandle, ref pioSendRequest,
				ref bytesToSend[0], bytesToSend.Length,
				ref pioSendRequest, ref recvBuff[0],
				ref expectedRequestLength);

			Logger.TraceEvent(TraceEventType.Verbose, 0, "SendAPDU ended. Return code: " + returnCode);
			Logger.Flush();

			//http://msdn.microsoft.com/en-us/library/windows/desktop/aa379804(v=vs.85).aspx
			//The pcbRecvLength should be at least n+2 and will be set to n+2 upon return.

			var result = new ApduResponse
			{
				RecvBuff = recvBuff,
				ResponseLength = expectedRequestLength,
				ReturnCode = returnCode
			};

			return result;
		}

		private IntPtr EstablishContextIfNotEstablished()
		{
			var establishContextResult = EstablishContext();
			if (!establishContextResult.IsSuccessful)
				throw new Exception(
					string.Format("Exception during context establishing for current thread ({0}). Error code: {1}",
						Thread.CurrentThread.ManagedThreadId, establishContextResult.StatusCode));

			var currentThreadContext = _contextManager.GetContext(Thread.CurrentThread.ManagedThreadId);

			return currentThreadContext;
		}

		/// <summary>
		/// Convert bytes structure to string list.
		/// </summary>
		private string[] ConvertReadersBuffer(byte[] readersBuffer)
		{
			IList<string> result = new List<string>();
			string readerName = "";
			int indx = 0;

			while (readersBuffer[indx] != 0)
			{
				while (readersBuffer[indx] != 0)
				{
					readerName = readerName + (char)readersBuffer[indx];
					indx = indx + 1;
				}

				//Add reader name to list
				result.Add(readerName);
				readerName = "";
				indx = indx + 1;
			}

			return result.ToArray();
		}


		/// <summary>
		/// Destructor.
		/// </summary>
		~LowLevelCardReader()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// The dispose method that implements IDisposable.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// The virtual dispose method that allows
		/// classes inherithed from this one to dispose their resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Dispose managed resources here.
				}

				ReleaseAllContexts();
			}

			_disposed = true;
		}
	}
}