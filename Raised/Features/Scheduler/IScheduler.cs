namespace Raised.Features
{
	internal interface IScheduler<T> : IDisposable
		where T : class, new()
	{
		void TryAdd(T item);
		void Remove(T item);
	}
}
