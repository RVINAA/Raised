using Raised.API.Facilities;

namespace Raised.API
{
	public interface ITelegramSender
	{
		void Send(string message);
	}

	public class TelegramSender : ITelegramSender
	{
		#region Fields

		private readonly ILogger<TelegramSender> _logger;
		private readonly IHttpClientService _httpClientSvc;
		private readonly TelegramSettings _settings;

		#endregion

		#region .ctors

		public TelegramSender(ILogger<TelegramSender> logger, IHttpClientService httpClientSvc, Settings settings)
		{
			_logger = logger.ThrowIfNull(nameof(logger));
			_settings = settings?.Telegram.ThrowIfNull(nameof(settings));
			_httpClientSvc = httpClientSvc.ThrowIfNull(nameof(httpClientSvc));
		}

		#endregion

		public void Send(string message)
		{
			Guard.IsNotNullOrWhiteSpace(message, nameof(message));

			var url = new UriBuilder(_settings.Url)
			{
				Path = string.Format("{0}/sendMessage", _settings.ApiToken),
				Query = string.Format("chat_id={0}&parse_mode=MarkdownV2&text={1}", _settings.ChannelId, message)
			};

			_httpClientSvc.Send(url.ToString(), HttpMethod.Get);
			_logger.LogInformation("Sent message to telegram channel");
		}
	}
}
