using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lando.LowLevel
{
	internal class ContextManager
	{
		private readonly object _locker = new object();

		private readonly ConcurrentDictionary<int, IntPtr> _threadContextDictionary = new ConcurrentDictionary<int, IntPtr>();
		private readonly ConcurrentDictionary<int, DateTime> _threadContextLog = new ConcurrentDictionary<int, DateTime>();

		public int Count
		{
			get { return _threadContextDictionary.Count; }
		}

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

		public IList<int> GetOldestContextOwnersThreads()
		{
			var result = _threadContextLog
				.Select(pair => pair)
				.OrderBy(x => x.Value)
				.Select(x => x.Key)
				.ToList();

			return result;
		}

		public void ContextReleased(int threadId)
		{
			IntPtr tmpPtr;
			DateTime tmpDt;

			_threadContextDictionary.TryRemove(threadId, out tmpPtr);
			_threadContextLog.TryRemove(threadId, out tmpDt);
		}
	}
}