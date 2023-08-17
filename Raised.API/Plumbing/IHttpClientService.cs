using System.Text.Json;

using Raised.API.Facilities;

namespace Raised.API
{
	public interface IHttpClientService : IDisposable
	{
		T Get<T>(string url, Action<HttpRequestMessage> customizer = null);
		string Send(string url, HttpMethod method, Action<HttpRequestMessage> customizer = null);
	}

	public sealed class HttpClientService : IHttpClientService
	{
		#region Fields

		private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(15);
		private static readonly JsonSerializerOptions _settings = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
			DefaultBufferSize = 2048,
			MaxDepth = 10
		};

		private readonly CancellationTokenSource _cancellationTokenSource = new();
		private readonly HttpClient _httpClient;
		private readonly ILogger _logger;

		#endregion

		#region .ctors

		public HttpClientService(ILogger<HttpClientService> logger)
		{
			Guard.IsNotNull(logger, nameof(logger));

			_logger = logger;
			_httpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromSeconds(30)
			};
		}

		#endregion

		public T Get<T>(string url, Action<HttpRequestMessage> customizer = null)
		{
			Guard.IsNotNull(url, nameof(url));

			try
			{
				using (var request = new HttpRequestMessage(HttpMethod.Get, url))
				{
					customizer?.Invoke(request);

					using (var response = _httpClient.SendAsync(request).WithTimeout(_timeout).GetAwaiter().GetResult())
					{
						return response.EnsureSuccessStatusCode().Content //< Throw if not OK.
							.ReadFromJsonAsync<T>(_settings, _cancellationTokenSource.Token)
							.GetAwaiter()
							.GetResult();
					}
				}
			}
			catch (Exception ex) //< TimeoutException, TaskCanceledException, HttpRequestException, JsonException.. meh.
			{
				_logger.LogError(ex, "Unexpected exception on GET request to url: '{url}'.", url);
				throw;
			}
		}

		public string Send(string url, HttpMethod method, Action<HttpRequestMessage> customizer = null)
		{
			Guard.IsNotNull(url, nameof(url));

			try
			{
				using (var request = new HttpRequestMessage(method, url))
				{
					customizer?.Invoke(request);

					using (var response = _httpClient.SendAsync(request).WithTimeout(_timeout).GetAwaiter().GetResult())
					{
						return response.EnsureSuccessStatusCode().Content //< Throw if not OK.
							.ReadAsStringAsync(_cancellationTokenSource.Token)
							.GetAwaiter()
							.GetResult();
					}
				}
			}
			catch (Exception ex) //< TimeoutException, TaskCanceledException, HttpRequestException, JsonException.. meh.
			{
				_logger.LogError(ex, "Unexpected exception on GET request to url: '{url}'.", url);
				throw;
			}
		}

		public void Dispose()
		{
			if (_cancellationTokenSource.IsCancellationRequested)
				return;

			_cancellationTokenSource.Cancel();
			_httpClient.Dispose();
		}
	}
}
