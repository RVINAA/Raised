﻿using System.Timers;
using System.Net.Http.Headers;

using Raised.Contracts;
using Raised.Facilities;

using Microsoft.Extensions.Logging;

using Timer = System.Timers.Timer;

namespace Raised.Features
{
	internal sealed class JenkinsJobScheduler : InMemoryScheduler<JenkinsJob>
	{
		#region Fields

		private const int DEF_INTERVAL = 600000; //< 10'.
		private const string DSL_ENCODING = "%252F";

		private readonly TelegramNotificationService _notificationSvc;
		private readonly IHttpClientService _httpClientSvc;
		private readonly ILogger _logger;

		#endregion

		#region .ctors

		public JenkinsJobScheduler(
			TelegramNotificationService notificationSvc,
			ILogger<JenkinsJobScheduler> logger,
			IHttpClientService httpClientSvc)
		{
			Guard.IsNotNull(notificationSvc, nameof(notificationSvc));
			Guard.IsNotNull(httpClientSvc, nameof(httpClientSvc));
			Guard.IsNotNull(logger, nameof(logger));

			_notificationSvc = notificationSvc;
			_httpClientSvc = httpClientSvc;
			_logger = logger;

		}

		#endregion

		#region Private methods

		public static void Populate(JenkinsJob item, ref DTOs.LastBuild obj)
		{
			Guard.IsNotNull(item, nameof(item));
			Guard.IsNotNull(obj, nameof(obj));

			item.Duration = obj.Duration;
			item.LastState = obj.Result switch
			{
				"SUCCESS" => JenkinsJob.State.Success,
				"FAILURE" => JenkinsJob.State.Failure,
				"ABORTED" => JenkinsJob.State.Aborted,
				"UNSTABLE" => JenkinsJob.State.Unstable,
				_ => throw new NotSupportedException($"Unexpected jenkins's outcome: {obj.Result}"),
			};

			if (obj.Actions != null)
			{
				foreach (var action in obj.Actions)
				{
					if (action.Class?.EndsWith("TestResultAction") == true)
						item.TestFailures = action.FailCount;
				}
			}
		}

		private static string BuildNotificationFor(JenkinsJob item)
		{
			Guard.IsNotNull(item, nameof(item));

			return $@"
				```
				╒═════════╤══════════════════════════════════╕
				│  STATS  │  Feeling lucky? It's your day ¶  │
				╞═════════╧══════════════════════════════════╛
				╞> Repository: {item.Repository}
				╞> Branch's name: {item.Branch}
				╞> Last state: {item.LastState}
				╞> Test failures: {item.TestFailures?.ToString() ?? "N/A"}
				╞> Took {item.Duration / 1000 / 60} minutes.
				╘> Scheduled: {(item.IsFailed ? "🗸" : "🞨")}
				```
			";
		}

		private static string GetFailureNotificationFor(JenkinsJob item, Exception ex)
		{
			Guard.IsNotNull(item, nameof(item));
			Guard.IsNotNull(ex, nameof(ex));

			return $@"
				```
				╒═════════╤══════════════════════════════════╕
				│  ERROR  │  Discarding job due failure.. †  │
				╞═════════╧══════════════════════════════════╛
				╞> Repository: {item.Repository}
				╞> Branch's name: {item.Branch}
				╘> Exception (?): {ex.Message ?? "N/A"}
				```
			";
		}

		#endregion

		protected override void CheckOnElapsed(object source, ElapsedEventArgs e, JenkinsJob item, Timer timer)
		{
			timer.Enabled = false;

			try
			{
				if (timer.Interval != DEF_INTERVAL)
					timer.Interval = DEF_INTERVAL;

				var auth = new AuthenticationHeaderValue(scheme: "Basic", parameter: item.B64);
				var url = $"{item.Url}/job/{item.Repository}/job/{item.Branch.Replace("/", DSL_ENCODING)}";
				var obj = _httpClientSvc.Get<DTOs.LastBuild>($"{url}/lastBuild/api/json", x => x.Headers.Authorization = auth);

				if (obj.Building) //< Do nothing..
					goto Restart;

				Populate(item, ref obj);

				_notificationSvc.Notify(item.ApiToken, item.Id, BuildNotificationFor(item));

				if (!item.IsFailed) //< Just schedule failed builds..
				{
					Remove(item);
					return;
				}

				_httpClientSvc.Send($"{url}/build", HttpMethod.Post, x => x.Headers.Authorization = auth);

			Restart:
				timer.Enabled = true;
			}
			catch (Exception ex)
			{
				SwallowExtensions.Execute(() => _notificationSvc.Notify(item.ApiToken, item.Id, GetFailureNotificationFor(item, ex)));
				_logger.LogError(ex, "Something has failed.. discarding job ({JenkinsJob.Repository} - {JenkinsJob.Branch}).", item.Repository, item.Branch);
				Remove(item);
			}
		}
	}
}
