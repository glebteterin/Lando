using System;

namespace Lando.LowLevel
{
	internal class Card
	{
		internal Guid InternalUid { get; set; }
		internal int ThreadId { get; set; }
		internal IntPtr ConnectionHandle { get; set; }
		internal string CardreaderName { get; set; }
		internal int Protocol { get; set; }

		internal byte[] Atr = null;
		internal byte[] IdBytes = null;

		public CardState State { get; set; }

		public Card()
		{}

		internal Card(int threadId, IntPtr connectionHandle, string cardreaderName, int protocol)
		{
			InternalUid = Guid.NewGuid();
			ThreadId = threadId;
			ConnectionHandle = connectionHandle;
			CardreaderName = cardreaderName;
			Protocol = protocol;
		}
	}
}