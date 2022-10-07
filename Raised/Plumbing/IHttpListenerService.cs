using System.Net;

using Raised.Facilities;

namespace Raised
{
	internal interface IHttpListenerService : IDisposable
	{
		delegate void Process(HttpListenerContext ctx);

		void Set(Process @delegate);
		void Add(string prefix);
		void Start();
	}

	internal sealed class HttpListenerService : IHttpListenerService
	{
		#region Fields

		private readonly HttpListener _httpListener = new();
		private IHttpListenerService.Process _process;
		private bool _disposed;
		private bool _started;

		#endregion

		public void Set(IHttpListenerService.Process @delegate) => _process = @delegate.ThrowIfNull(nameof(@delegate));

		public void Add(string prefix)
		{
			var obj = _httpListener;
			Guard.IsNotNull(obj, nameof(obj));
			Guard.Against<InvalidOperationException>(obj.IsListening, "Cannot add prefixes because the listener is running."); //< No idea if I can.. so throw.

			obj.Prefixes.Add(prefix);
		}

		public void Start()
		{
			if (_started || _disposed)
				return;

			_httpListener.Start();
			_started = true;

			while (true)
			{
				var ctx = _httpListener.GetContext();
				_process(ctx);
			}
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_httpListener.Abort();
			_disposed = true;
		}
	}
}
