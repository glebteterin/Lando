using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Lando.LowLevel
{
	internal class CardConnectionManager
	{
		private readonly object _locker = new object();

		private readonly ConcurrentDictionary<string, IntPtr> _connectionContextDictionary = new ConcurrentDictionary<string, IntPtr>();

		public bool IsConnectionExist(Guid cardId, int threadId)
		{
			lock (_locker)
			{
				var key = GetKey(cardId, threadId);

				return _connectionContextDictionary.ContainsKey(key);
			}
		}

		public IntPtr GetConnection(Guid cardId, int threadId)
		{
			lock (_locker)
			{
				IntPtr result = IntPtr.Zero;

				var key = GetKey(cardId, threadId);

				_connectionContextDictionary.TryGetValue(key, out result);

				return result;
			}
		}

		public void AddConnection(Guid cardId, int threadId, IntPtr context)
		{
			var key = GetKey(cardId, threadId);

			_connectionContextDictionary.AddOrUpdate(key, context, (keyParam, newPointer) => context);
		}

		public void CardDisconnected(Guid cardId)
		{
			lock (_locker)
			{
				var keyPart = cardId.ToString();
				var allKeys = _connectionContextDictionary.Keys;

				var keysToRemove = allKeys.Where(x => x.StartsWith(keyPart)).ToArray();

				foreach (var keyToRemove in keysToRemove)
				{
					IntPtr tmp;

					_connectionContextDictionary.TryRemove(keyToRemove, out tmp);
				}
			}
		}

		private string GetKey(Guid cardId, int threadId)
		{
			var key = cardId.ToString() + threadId;
			return key;
		}
	}
}