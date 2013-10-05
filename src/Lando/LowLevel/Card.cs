using System;

namespace Lando.LowLevel
{
	internal class Card
	{
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
			ThreadId = threadId;
			ConnectionHandle = connectionHandle;
			CardreaderName = cardreaderName;
			Protocol = protocol;
		}
	}
}