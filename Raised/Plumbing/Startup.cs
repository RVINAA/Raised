using Raised.Features;
using Raised.Facilities;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Raised
{
	internal static class Startup
	{
		#region Private methods

		private static void Customizer(IServiceCollection services)
		{
			Guard.IsNotNull(services, nameof(services));

			var cfg = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false)
				.Build();

			services.AddSingleton(cfg)
				.AddSingleton<IJenkinsJobSchedulerSettings>(_ => cfg.GetSection(nameof(JenkinsJobSchedulerSettings)).Get<JenkinsJobSchedulerSettings>())
				.AddSingleton<IJenkinsJobWatcherSettings>(_ => cfg.GetSection(nameof(JenkinsJobWatcherSettings)).Get<JenkinsJobWatcherSettings>())
				.AddScoped<IHttpListenerService, HttpListenerService>()
				.AddSingleton<IHttpClientService, HttpClientService>()
				.AddSingleton<IWatcher, JenkinsJobWatcher>()
				.AddSingleton<TelegramNotificationService>()
				.AddSingleton<JenkinsJobScheduler>();
		}

		#endregion

		public static IHost GetHost() =>
			Host.CreateDefaultBuilder()
				.ConfigureLogging(x =>
					x.AddSimpleConsole(y =>
					{
						y.TimestampFormat = "hh:mm:ss.ms - ";
						y.UseUtcTimestamp = true;
						y.SingleLine = true;
					})
					.SetMinimumLevel(LogLevel.Debug))
				.ConfigureServices(Customizer)
				.Build();

		public static IHost Initialize(this IHost @this)
		{
			Guard.IsNotNull(@this, nameof(@this));

			var svc = @this.Services;
			var watchers = svc.GetServices<IWatcher>();
			foreach (var watcher in watchers)
			{
				watcher.Init();
			}

			return @this;
		}
	}
}
