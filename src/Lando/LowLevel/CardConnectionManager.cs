using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

		public IntPtr[] GetConnections(Guid cardId)
		{
			lock (_locker)
			{
				var result = new List<IntPtr>();

				var keyPart = cardId.ToString();
				var allKeys = _connectionContextDictionary.Keys;

				var cardKeys = allKeys.Where(x => x.StartsWith(keyPart)).ToArray();

				foreach (var keyToRemove in cardKeys)
				{
					IntPtr tmp;

					if (_connectionContextDictionary.TryGetValue(keyToRemove, out tmp))
					{
						result.Add(tmp);
					}
				}

				return result.ToArray();
			}
		}

		public void AddConnection(Guid cardId, int threadId, IntPtr context)
		{
			var key = GetKey(cardId, threadId);

			_connectionContextDictionary.AddOrUpdate(key, context, (keyParam, newPointer) => context);
		}

		/// <summary>
		/// Forget given card and all it's handles.
		/// </summary>
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