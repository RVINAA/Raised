using Raised.Facilities;

namespace Raised
{
	internal class TelegramNotificationService
	{
		#region Fields

		// TODO?: May this shouldn't be hardcoded, but anything more is supported.
		private const string TELEGRAM_API_URL = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&parse_mode=MarkdownV2&text={2}";

		private readonly IHttpClientService _httpClientSvc;

		#endregion

		#region .ctors

		public TelegramNotificationService(IHttpClientService httpClientSvc)
		{
			Guard.IsNotNull(httpClientSvc, nameof(httpClientSvc));
			_httpClientSvc = httpClientSvc;
		}

		#endregion

		public void Notify(string apiToken, string id, string message)
		{
			Guard.IsNotNullOrWhiteSpace(apiToken, nameof(apiToken));
			Guard.IsNotNullOrWhiteSpace(message, nameof(message));
			Guard.IsNotNullOrWhiteSpace(id, nameof(id));

			_httpClientSvc.Send(string.Format(TELEGRAM_API_URL, apiToken, id, message), HttpMethod.Get);
		}
	}
}
