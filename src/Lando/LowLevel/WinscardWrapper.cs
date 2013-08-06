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

		// Context scope

		/// <summary>
		/// Database operations are performed within the domain of the user.
		/// </summary>
		public const uint SCARD_SCOPE_USER = 0;

		/// <summary>
		/// This application will share this card with other applications.
		/// </summary>
		public const uint SCARD_SHARE_SHARED = 2;

		// Protocols

		/// <summary>
		/// T=0 is the active protocol.
		/// </summary>
		public const int SCARD_PROTOCOL_T0 = 0x01;
		/// <summary>
		/// T=1 is the active protocol.
		/// </summary>
		public const int SCARD_PROTOCOL_T1 = 0x02;

		// Smart card status

		/// <summary>
		/// No error was encountered.
		/// </summary>
		public const int SCARD_S_SUCCESS = 0;
		/// <summary>
		/// This value implies the driver is unaware of the current state of the reader.
		/// </summary>
		public const int SCARD_UNKNOWN = 0;
		/// <summary>
		/// There is no card in the reader.
		/// </summary>
		public const int SCARD_ABSENT = 1;
		/// <summary>
		/// There is a card in the reader, but it has not been moved into position for use.
		/// </summary>
		public const int SCARD_PRESENT = 2;
		/// <summary>
		/// There is a card in the reader in position for use. The card is not powered.
		/// </summary>
		public const int SCARD_SWALLOWED = 3;
		/// <summary>
		/// Power is being provided to the card, but the reader driver is unaware of the mode of the card.
		/// </summary>
		public const int SCARD_POWERED = 4;
		/// <summary>
		/// The card has been reset and is awaiting PTS negotiation.
		/// </summary>
		public const int SCARD_NEGOTIABLE = 5;
		/// <summary>
		/// The card has been reset and specific communication protocols have been established.
		/// </summary>
		public const int SCARD_SPECIFIC = 6;

		// Methods

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

		/// <see cref="http://msdn.microsoft.com/en-us/library/ms936965.aspx"/>
		/// <summary>
		/// Returns description of error code.
		/// </summary>
		public static OperationResult GetErrorMessage(int code)
		{
			if (code == SCARD_S_SUCCESS)
				return new OperationResult(true, code, "SCARD_S_SUCCESS", " No error was encountered.");

			return new OperationResult(false, code, "UNKNOWN", "");
		}

	}
}