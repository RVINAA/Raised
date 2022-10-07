using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Raised
{
	internal class Program
	{
		#region Fields

		private static ILogger _logger;

		#endregion

		public static int Main()
		{
			try
			{
				var host = Startup.GetHost();

				_logger = host.Services.GetRequiredService<ILogger<Program>>();
				_logger.LogInformation("Configuration done.. host created succesfully.");

				host.Initialize().Run();

				_logger.LogInformation("Tearing down.. may without issues.");
			}
			catch (Exception ex)
			{
				_logger?.LogCritical(ex, "Oops, fatality!");
				return 255;
			}

			return 0;
		}
	}
}

