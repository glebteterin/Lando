using System;

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