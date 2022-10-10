using System.Timers;
using System.Collections.Concurrent;

using Raised.Facilities;

using Microsoft.Extensions.Logging;

using Timer = System.Timers.Timer;

namespace Raised.Features
{
	internal abstract class InMemoryScheduler<T> : IScheduler<T>
		where T : class, IEquatable<T>, new()
	{
		#region Fields

		protected const int INI_INTERVAL = 10000; //< 10".

		private readonly ConcurrentDictionary<T, Timer> _items = new();
		private readonly ILogger _logger;

		private bool _disposed;

		#endregion

		#region .ctors

		protected InMemoryScheduler(ILogger<InMemoryScheduler<T>> logger)
		{
			Guard.IsNotNull(logger, nameof(logger));
			_logger = logger;
		}

		#endregion

		protected abstract void CheckOnElapsed(object source, ElapsedEventArgs e, T item, Timer timer);

		public void TryAdd(T item)
		{
			Guard.IsNotNull(item, nameof(item));

			var name = typeof(T).Name;
			var timer = new Timer()
			{
				Interval = INI_INTERVAL,
				Enabled = true,
			};

			_logger.LogInformation("Trying to schedule a new item of type {Item.Name}..", name);

			if (_items.TryAdd(item, timer))
			{
				_logger.LogInformation("Scheduled a new item of type {Item.Name}", name);
				timer.Elapsed += (sender, e) => CheckOnElapsed(sender, e, item, timer);
				timer.Start();
			}
		}

		public void Remove(T item)
		{
			Guard.IsNotNull(item, nameof(item));

			_logger.LogInformation("Trying to remove from scheduler an item of type {Item.Name}..", typeof(T).Name);

			if (_items.Remove(item, out var timer))
			{
				_logger.LogInformation("Removed from scheduler an item of type {Item.Name}.", typeof(T).Name);
				timer.Dispose();
			}
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
