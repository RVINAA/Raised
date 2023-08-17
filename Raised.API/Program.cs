using Hellang.Middleware.ProblemDetails;

using Raised.API.Features;

namespace Raised.API
{
	public class Program
	{
		private static bool _isDevelopment;

		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddProblemDetails(x =>
			{
				x.IncludeExceptionDetails = (ctx, env) => _isDevelopment;
			});
			builder.Services.AddControllers();
			builder.Services.AddSwaggerGen();

			var cfg = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true) //load local settings
				.AddEnvironmentVariables()
				.Build();

			builder.Services.AddSingleton(cfg)
				.AddSingleton(_ => cfg.GetSection("Settings").Get<Settings>())
				//.AddSingleton<ITelegramSettings>(_ => cfg.GetSection("ITelegramSettings").Get<ITelegramSettings>())
				//.AddSingleton<IJenkinsSettings>(_ => cfg.GetSection("JenkinsSettings").Get<IJenkinsSettings>())
				.AddSingleton<IJenkinsScheduleManager, JenkinsScheduleManager>()
				.AddSingleton<IHttpClientService, HttpClientService>()
				.AddSingleton<ITelegramSender, TelegramSender>();

			builder.Logging
				.AddSimpleConsole(x =>
				{
					x.TimestampFormat = "hh:mm:ss.ms - ";
					x.UseUtcTimestamp = true;
					x.SingleLine = true;
				});

			var app = builder.Build();
			if (_isDevelopment = app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();
			app.UseProblemDetails();
			app.UseAuthorization();
			app.MapControllers();
			app.Run();
		}
	}
}
