using System.Net;
using System.Text;
using System.Text.Json;

using Raised.Contracts;
using Raised.Facilities;

using Microsoft.Extensions.Logging;

namespace Raised.Features
{
	internal class JenkinsJobWatcher : IWatcher
	{
		#region Fields

		private static readonly JsonSerializerOptions _jsonSettings = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
			DefaultBufferSize = 2048,
			MaxDepth = 2
		};

		private readonly IHttpListenerService _httpListenerSvc;
		private readonly JenkinsJobScheduler _scheduler;
		private readonly IJenkinsJobWatcherSettings _settings;
		private readonly ILogger _logger;

		private Thread _thread;
		private bool _disposed;
		private bool _started;

		#endregion

		#region .ctors

		public JenkinsJobWatcher(
			IHttpListenerService httpListenerSvc,
			ILogger<JenkinsJobWatcher> logger,
			JenkinsJobScheduler scheduler,
			IJenkinsJobWatcherSettings settings)
		{
			Guard.IsNotNull(httpListenerSvc, nameof(httpListenerSvc));
			Guard.IsNotNull(scheduler, nameof(scheduler));
			Guard.IsNotNull(settings, nameof(settings));
			Guard.IsNotNull(logger, nameof(logger));

			_httpListenerSvc = httpListenerSvc;
			_scheduler = scheduler;
			_settings = settings;
			_logger = logger;
		}

		#endregion

		#region Private methods

		private void Process(HttpListenerContext ctx)
		{
			_logger.LogInformation("HTTP request received.. we'll try to process it.");

			using (var res = ctx.Response)
			{
				var req = ctx.Request;
				if (req.HttpMethod != "POST")
				{
					res.StatusDescription = "Sorry dude, I just like POST verb.";
					res.StatusCode = (int)HttpStatusCode.BadRequest;
					return;
				}

				if (req.InputStream == Stream.Null)
				{
					res.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
					res.StatusDescription = "Missing body..";
					return;
				}

				using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
				{
					try
					{
						var obj = JsonSerializer.Deserialize<JenkinsJob>(reader.ReadToEnd(), _jsonSettings);
						obj.B64 = req.Headers["Authorization"].ThrowIfNullOrWhiteSpace("Basic Auth").Replace("Basic", "").Trim(); //< Ugly as fuck..

						Guard.IsNotNullOrWhiteSpace(obj.Repository, nameof(obj.Repository));
						Guard.IsNotNullOrWhiteSpace(obj.Branch, nameof(obj.Branch));
						Guard.IsNotNullOrWhiteSpace(obj.Url, nameof(obj.Url));

						if (obj.Cancel != true)
						{
							_scheduler.TryAdd(obj);
						}
						else
						{
							_scheduler.RemoveIfNeeded(obj);
						}

						res.StatusDescription = "Seems good.. if invalid, you'll know.";
						res.StatusCode = (int)HttpStatusCode.OK;

						_logger.LogInformation("HTTP request processed sucessfully (branch: {JenkinsJob.Branch}, repository: {JenkinsJob.Repository}).", obj.Branch, obj.Repository);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Failure trying to deserialize the current request..");
						res.StatusDescription = "Your JSON / req sucks (it's not valid).";
						res.StatusCode = (int)HttpStatusCode.UnprocessableEntity;

						var buffer = Encoding.UTF8.GetBytes(ex.ToString());
						res.ContentLength64 = buffer.Length;
						using Stream ros = res.OutputStream;
						ros.Write(buffer, 0, buffer.Length);
					}
				}
			}
		}

		#endregion

		public void Init()
		{
			if ( _started || _disposed)
				return;

			_logger.LogInformation("Trying to wire watcher listening on: '{url}'..", _settings.EndPointPrefix);
			_thread = new Thread(() =>
			{
				_httpListenerSvc.Add(_settings.EndPointPrefix);
				_httpListenerSvc.Set(Process);
				_httpListenerSvc.Start();
			})
			{
				Name = nameof(JenkinsJobWatcher)
			};

			_thread.Start();
			_started = true;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_httpListenerSvc.Dispose();
			_disposed = true;
		}
	}
}
