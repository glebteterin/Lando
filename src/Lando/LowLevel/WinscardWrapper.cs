using System;
using System.Runtime.InteropServices;

namespace Lando.LowLevel
{
	internal class WinscardWrapper
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct SCARD_IO_REQUEST
		{
			public int dwProtocol;
			public int cbPciLength;
		}

		/// <summary>
		/// Used by functions for tracking smart cards within readers.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct SCARD_READERSTATE
		{
			public string szReaderName;
			public IntPtr pvUserData;
			public int dwCurrentState;
			public int dwEventState;
			public int cbAtr;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
			public byte[] rgbAtr;
		}
		
		/// <summary>
		/// The SCardEstablishContext function establishes the resource manager context (the scope) within which database operations are performed.
		/// </summary>
		[DllImport("winscard.dll")]
		public static extern int SCardEstablishContext(uint dwScope, IntPtr pvReserved1, IntPtr pvReserved2, out IntPtr phContext);

		/// <summary>
		/// This function closes an established resource manager context, freeing any resources allocated under that context.
		/// </summary>
		[DllImport("winscard.dll")]
		public static extern int SCardReleaseContext(IntPtr phContext);

		/// <summary>
		/// The SCardConnect function establishes a connection (using a specific resource manager context) between the calling application and a smart card contained by a specific reader. If no card exists in the specified reader, an error is returned.
		/// </summary>
		[DllImport("winscard.dll")]
		public static extern int SCardConnect(IntPtr hContext, string szReaderName, uint dwShareMode, uint dwPreferredProtocols, out IntPtr phCard, out int pdwActiveProtocol);

		/// <summary>
		/// The SCardDisconnect function terminates a connection previously opened between the calling application and a smart card in the target reader.
		/// </summary>
		[DllImport("winscard.dll")]
		public static extern int SCardDisconnect(IntPtr hCard, int dwDisposition);

		/// <summary>
		/// The SCardListReaders function provides the list of readers within a set of named reader groups, eliminating duplicates.
		/// </summary>
		[DllImport("winscard.DLL", EntryPoint = "SCardListReadersA", CharSet = CharSet.Ansi)]
		public static extern int SCardListReaders(IntPtr hContext, byte[] mszGroups, byte[] mszReaders, ref int pcchReaders);

		/// <summary>
		/// The SCardStatus function provides the current status of a smart card in a reader. You can call it any time after a successful call to SCardConnect and before a successful call to SCardDisconnect. It does not affect the state of the reader or reader driver.
		/// </summary>
		[DllImport("winscard.dll")]
		public static extern int SCardStatus(IntPtr hCard, string szReaderName, ref int pcchReaderLen, ref int pdwState, ref int pdwProtocol, ref byte pbAtr, ref int pcbAtrLen);

		/// <summary>
		/// The SCardTransmit function sends a service request to the smart card and expects to receive data back from the card.
		/// </summary>
		[DllImport("winscard.dll")]
		public static extern int SCardTransmit(IntPtr hCard, ref SCARD_IO_REQUEST pioSendPci, ref byte pbSendBuffer, int cbSendLength, ref SCARD_IO_REQUEST pioRecvPci, ref byte pbRecvBuffer, ref int pcbRecvLength);

		/// <summary>
		/// The SCardGetStatusChange function blocks execution until the current availability of the cards in a specific set of readers changes.
		/// </summary>
		[DllImport("winscard.dll")]
		public static extern int SCardGetStatusChange(IntPtr phContext, uint dwTimeout, [In, Out] SCARD_READERSTATE[] rgReaderStates, int cReaders);
	}
}