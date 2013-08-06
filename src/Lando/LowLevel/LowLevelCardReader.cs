using System;
using System.Collections.Generic;
using System.Linq;
using Lando.LowLevel.ResultsTypes;

namespace Lando.LowLevel
{
	internal class LowLevelCardReader : IDisposable
	{
		private IntPtr _resourceManagerContext = IntPtr.Zero;

		private bool _isConnected;

		private bool _disposed;

		/// <summary>
		/// Establish Context of Resource Manager.
		/// </summary>
		public OperationResult EstablishContext()
		{
			if (_resourceManagerContext != IntPtr.Zero)
				return new OperationResult(true, WinscardWrapper.SCARD_S_SUCCESS, null, null);

			IntPtr notUsed1 = IntPtr.Zero;
			IntPtr notUsed2 = IntPtr.Zero;

			int returnCode = WinscardWrapper.SCardEstablishContext(WinscardWrapper.SCARD_SCOPE_USER,
																	notUsed1,
																	notUsed2,
																	out _resourceManagerContext);

			if (returnCode == WinscardWrapper.SCARD_S_SUCCESS)
				_isConnected = true;

			return WinscardWrapper.GetErrorMessage(returnCode);
		}

		public int ReleaseContext()
		{
			int returnCode = WinscardWrapper.SCardReleaseContext(_resourceManagerContext);

			return returnCode;
		}

		/// <summary>
		/// Return card readers list.
		/// </summary>
		public OperationResult GetCardReadersList(out string[] readersList)
		{
			if (!_isConnected)
				throw new InvalidOperationException("You cannot call this method without esbablished context. Try call EstablishContext method first.");

			readersList = new string[0];

			OperationResult result;
			int sizeOfReadersListStructure = 0;

			int returnCode = WinscardWrapper.SCardListReaders(_resourceManagerContext, null, null, ref sizeOfReadersListStructure);

			if (returnCode != WinscardWrapper.SCARD_S_SUCCESS)
			{
				result = WinscardWrapper.GetErrorMessage(returnCode);
			}
			else
			{
				// Fill reader list
				var cardReadersList = new byte[sizeOfReadersListStructure];
				returnCode = WinscardWrapper.SCardListReaders(_resourceManagerContext, null, cardReadersList, ref sizeOfReadersListStructure);

				if (returnCode != WinscardWrapper.SCARD_S_SUCCESS)
				{
					result = WinscardWrapper.GetErrorMessage(returnCode);
				}
				else
				{
					// Convert to strings
					readersList = ConvertReadersBuffer(cardReadersList);

					result = WinscardWrapper.GetErrorMessage(returnCode);
				}
			}

			return result;
		}

		/// <summary>
		/// Establishing a connection to smart card contained by a specific reader.
		/// <param name="cardreaderName">Card reader name to connection.</param>
		/// </summary>
		public ConnectResult Connect(string cardreaderName)
		{
			if (!_isConnected)
				throw new InvalidOperationException("You cannot call this method without esbablished context. "
													+ "Try call EstablishContext method first.");

			IntPtr cardConnectionHandle;
			int connectionProtocolType;

			int returnCode = WinscardWrapper.SCardConnect(
				_resourceManagerContext,
				cardreaderName,
				WinscardWrapper.SCARD_SHARE_SHARED,
				WinscardWrapper.SCARD_PROTOCOL_T0 | WinscardWrapper.SCARD_PROTOCOL_T1,
				out cardConnectionHandle,
				out connectionProtocolType);

			var operationResult = WinscardWrapper.GetErrorMessage(returnCode);
			var connectResult = new ConnectResult(operationResult);

			if (operationResult.IsSuccessful)
				connectResult.ConnectedCard = new Card(cardConnectionHandle, cardreaderName, connectionProtocolType);

			return connectResult;
		}

		/// <summary>
		/// The function provides the current status of a smart card in a reader.
		/// </summary>
		public bool GetCardState(Card cardToRead)
		{
			if (cardToRead == null) throw new ArgumentNullException("cardToRead");

			bool result;

			var sizeOfReadersListStructure = 0;
			var cardStateStatus = 0;
			var dwActProtocol = 0;
			var tmpAtrBytes = new byte[257];
			var tmpAtrLen = 32;

			var returnCode = WinscardWrapper.SCardStatus(cardToRead.ConnectionHandle, cardToRead.CardreaderName, ref sizeOfReadersListStructure, ref cardStateStatus, ref dwActProtocol, ref tmpAtrBytes[0], ref tmpAtrLen);

			if (returnCode != WinscardWrapper.SCARD_S_SUCCESS)
			{
				result = ReturnCodeManager.GetCardState(returnCode);
			}
			else
			{
				cardToRead.State = new CardState(cardStateStatus);
				cardToRead.Atr = tmpAtrBytes.Take(tmpAtrLen).ToArray();
				cardToRead.Protocol = dwActProtocol;
				result = ReturnCodeManager.GetCardState(returnCode);
			}

			return result;
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

				if (_isConnected)
					ReleaseContext();
			}

			_disposed = true;
		}
	}
}