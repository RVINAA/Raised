using System.Timers;
using System.Collections.Concurrent;

using Raised.Facilities;

using Timer = System.Timers.Timer;

namespace Raised.Features
{
	internal abstract class InMemoryScheduler<T> : IScheduler<T>
		where T : class, IEquatable<T>, new()
	{
		#region Fields

		protected const int INI_INTERVAL = 10000; //< 10".

		private readonly ConcurrentDictionary<T, Timer> _items = new();
		private bool _disposed;

		#endregion

		protected abstract void CheckOnElapsed(object source, ElapsedEventArgs e, T item, Timer timer);

		public void TryAdd(T item)
		{
			Guard.IsNotNull(item, nameof(item));

			var timer = new Timer()
			{
				Interval = INI_INTERVAL,
				Enabled = true,
			};

			_items.TryAdd(item, timer); //< XXX: Item's equality impl will handle this.
			timer.Elapsed += (sender, e) => CheckOnElapsed(sender, e, item, timer);
			timer.Start();
		}

		public void Remove(T item)
		{
			Guard.IsNotNull(item, nameof(item));
			_items.Remove(item, out var timer);
			timer.Dispose();
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			foreach (var item in _items)
				item.Value.Dispose();
		}
	}
}
