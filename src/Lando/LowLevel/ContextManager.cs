using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lando.LowLevel
{
	internal class ContextManager
	{
		private readonly object _locker = new object();

		private readonly ConcurrentDictionary<int, IntPtr> _threadContextDictionary = new ConcurrentDictionary<int, IntPtr>();

		public bool IsContextExist(int threadId)
		{
			lock (_locker)
			{
				return _threadContextDictionary.ContainsKey(threadId);
			}
		}

		public IntPtr GetContext(int threadId)
		{
			lock (_locker)
			{
				IntPtr result = IntPtr.Zero;

				_threadContextDictionary.TryGetValue(threadId, out result);

				return result;
			}
		}

		public void AddContext(int threadId, IntPtr context)
		{
			_threadContextDictionary.AddOrUpdate(threadId, context, (threadIdParam, newPointer) => context);
		}

		public ICollection<int> GetAllThreads()
		{
			return _threadContextDictionary.Keys;
		}

		public void ContextReleased(int threadId)
		{
			IntPtr tmp;

			_threadContextDictionary.TryRemove(threadId, out tmp);
		}
	}
}