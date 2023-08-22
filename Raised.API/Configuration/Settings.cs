using System.ComponentModel.DataAnnotations;

namespace Raised.API
{
	public class JenkinsSettings
	{
		/// <summary>
		/// Jenkins's url.
		/// </summary>
		[Url]
		[Required]
		public string Url { get; init; }

		/// <summary>
		/// Jenkins's username in order to curl a job request with basic auth.
		/// </summary>
		[Required]
		public string Username { get; init; }

		/// <summary>
		/// Jenkins's apiToken in order to curl a job request with basic auth.
		/// </summary>
		[Required]
		public string ApiToken { get; init;}

		/// <summary>
		/// Interval in milliseconds to configure the schedule timer.
		/// </summary>
		public TimeSpan ScheduleDelay { get; init; } = TimeSpan.FromMinutes(5);

		public string LastBuildPath { get; init; } = "job/{0}/job/{1}/lastBuild/api/json";

		public string RebuildPath { get; init; } = "job/{0}/job/{1}/build";

		public string TestReportPath { get; init; } = "job/{0}/job/{1}/lastBuild/testReport/api/json?tree=suites[name,cases[name,status]]";
	}

	public class TelegramSettings
	{
		/// <summary>
		/// Telegram's API url.
		/// </summary>
		[Url]
		[Required]
		public string Url { get; init; }

		[Required]
		public string ApiToken { get; init;}

		[Required]
		public string ChannelId { get; init; }
	}

	public class Settings
	{
		public JenkinsSettings Jenkins { get; init; }

		public TelegramSettings Telegram { get; init; }
	}
}
