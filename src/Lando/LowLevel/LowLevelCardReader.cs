using System;
using System.Collections.Generic;
using System.Linq;

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
		public int EstablishContext()
		{
			IntPtr notUsed1 = IntPtr.Zero;
			IntPtr notUsed2 = IntPtr.Zero;

			int returnCode = WinscardWrapper.SCardEstablishContext(WinscardWrapper.SCARD_SCOPE_USER,
																	notUsed1,
																	notUsed2,
																	out _resourceManagerContext);

			if (returnCode == 0)
				_isConnected = true;

			return returnCode;
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