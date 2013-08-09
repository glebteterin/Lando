using System.Collections.Generic;

namespace Lando.LowLevel
{
	internal class CardreaderPnpStatus : CardreaderStatus
	{
		private const string NewCardreadersNotification = "\\\\?PnP?\\Notification";

		private readonly int _readersNumber;

		public CardreaderPnpStatus(int readersNumber): base(NewCardreadersNotification)
		{
			_readersNumber = readersNumber;
		}

		public override WinscardWrapper.SCARD_READERSTATE ToScardStatus()
		{
			return new WinscardWrapper.SCARD_READERSTATE
			{
				szReaderName = NewCardreadersNotification,
				dwCurrentState = (_readersNumber << 16),
				dwEventState = 0
			};
		}
	}

	internal class CardreaderStatus
	{
		public string Name { get; set; }

		public int CurrentStatusFlags { get; set; }

		public int NewStatusFlags { get; set; }

		public bool IsChanged
		{
			get { return CurrentStatusFlags != NewStatusFlags; }
		}

		public StatusType[] Statuses
		{
			get { return DetectState(); }
		}

		public CardreaderStatus(string cardreaderName)
		{
			Name = cardreaderName;
			CurrentStatusFlags = WinscardWrapper.SCARD_STATE_UNAWARE;
		}

		public virtual WinscardWrapper.SCARD_READERSTATE ToScardStatus()
		{
			return new WinscardWrapper.SCARD_READERSTATE
				{
					szReaderName = Name,
					dwCurrentState = CurrentStatusFlags,
					dwEventState = 0
				};
		}

		public void Swap()
		{
			CurrentStatusFlags = NewStatusFlags;
		}

		private bool CheckFlag(int value, int valueToCheck)
		{
			return (value & valueToCheck) == valueToCheck;
		}

		private StatusType[] DetectState()
		{
			var detectedStatuses = new List<StatusType>();

			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_CHANGED))
				detectedStatuses.Add(StatusType.Changed);

			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_UNAVAILABLE))
				detectedStatuses.Add(StatusType.CardreaderDisconnected);

			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_IGNORE))
				detectedStatuses.Add(StatusType.CardreaderDisconnected);

			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_PRESENT))
				detectedStatuses.Add(StatusType.CardConnected);

			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_EMPTY))
				detectedStatuses.Add(StatusType.CardDisconnected);
			
			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_UNKNOWN))
				detectedStatuses.Add(StatusType.Unknown);

			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_ATRMATCH))
				detectedStatuses.Add(StatusType.AtrMatch);

			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_EXCLUSIVE))
				detectedStatuses.Add(StatusType.CardExclusive);

			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_INUSE))
				detectedStatuses.Add(StatusType.CardInUse);

			if (CheckFlag(NewStatusFlags, WinscardWrapper.SCARD_STATE_MUTE))
				detectedStatuses.Add(StatusType.CardUnresponsive);

			return detectedStatuses.ToArray();
		}

		public enum StatusType
		{
			Changed,
			CardreaderDisconnected,
			CardConnected,
			CardDisconnected,
			Unknown,
			AtrMatch,
			CardExclusive,
			CardInUse,
			CardUnresponsive,
		}
	}
}