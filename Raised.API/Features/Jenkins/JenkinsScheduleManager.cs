using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Timers;

using Raised.API.Facilities;

using Timer = System.Threading.Timer;

namespace Raised.API.Features
{
	using State = JenkinsScheduleState;

	public interface IJenkinsScheduleManager : IDisposable
	{
		bool Exist(Guid scheduleId);
		bool Exist(string repository, string branch);

		Guid Create(string repository, string branch, bool metrics);
		void Remove(Guid scheduleId);
	}

	public class JenkinsScheduleManager : IJenkinsScheduleManager
	{
		#region Fields

		private const string DSL_ENCODING = "%252F";

		private readonly ConcurrentDictionary<(Guid, string, string), State> _schedules = new();
		private readonly CancellationTokenSource _cts = new();

		private readonly ILogger<JenkinsScheduleManager> _logger;

		private readonly IJenkinsScheduleStatsGatherer _statsGatherer;
		private readonly IHttpClientService _httpClientSvc;
		private readonly ITelegramSender _telegram;
		private readonly JenkinsSettings _settings;

		private AuthenticationHeaderValue _authHeader;

		#endregion

		#region .ctors

		public JenkinsScheduleManager(
			ILogger<JenkinsScheduleManager> logger,
			IJenkinsScheduleStatsGatherer statsGatherer,
			IHttpClientService httpClientSvc,
			ITelegramSender telegram,
			Settings settings)
		{
			_logger = logger.ThrowIfNull(nameof(logger));
			_telegram = telegram.ThrowIfNull(nameof(telegram));
			_settings = settings?.Jenkins.ThrowIfNull(nameof(settings));
			_httpClientSvc = httpClientSvc.ThrowIfNull(nameof(httpClientSvc));
			_statsGatherer = statsGatherer.ThrowIfNull(nameof(statsGatherer));
		}

		#endregion

		#region Private methods

		private AuthenticationHeaderValue GetAuthHeader()
			=> _authHeader ??= new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.Username}:{_settings.ApiToken}")));

		private JenkinsLastBuild GetLastBuildFor(State state)
		{
			Guard.IsNotNull(state, nameof(state));

			var url = new Uri(_settings.Url);
			var branch = state.Schedule.Branch.Replace("/", DSL_ENCODING);
			url = new Uri(url, string.Format(_settings.LastBuildPath, state.Schedule.Repository, branch));

			return _httpClientSvc.Get<JenkinsLastBuild>(url.ToString(), x => x.Headers.Authorization = GetAuthHeader());
		}

		private void Dump(JenkinsLastBuild dto, State state)
		{
			Guard.IsNotNull(dto, nameof(dto));
			Guard.IsNotNull(state, nameof(state));

			state.Revision = dto.Number;
			state.LastDuration = TimeSpan.FromMilliseconds(dto.Duration);
			state.LastState = dto.Result switch
			{
				"SUCCESS" => State._State.Success,
				"FAILURE" => State._State.Failure,
				"ABORTED" => State._State.Aborted,
				"UNSTABLE" => State._State.Unstable,
				_ => throw new NotSupportedException($"Unexpected jenkins's outcome: {dto.Result}"),
			};

			if (dto.Actions != null)
			{
				foreach (var action in dto.Actions)
				{
					if (action.Class?.EndsWith("TestResultAction") == true)
						state.TestFailures = action.FailCount;
				}
			}

			_logger.LogDebug("Updated state {0} with last result", state);
		}

		private void MayCollect(JenkinsLastBuild dto, State state)
		{
			Guard.IsNotNull(dto, nameof(dto));
			Guard.IsNotNull(state, nameof(state));

			if (!state.Schedule.Collect)
				return;

			JenkinsLastTestReports testReports = null;
			if (state.TestFailures > 0) //< Fetch this if at least one test has failed.
			{
				var url = new Uri(_settings.Url);
				var branch = state.Schedule.Branch.Replace("/", DSL_ENCODING);
				url = new Uri(url, string.Format(_settings.TestReportPath, state.Schedule.Repository, branch));
				testReports = _httpClientSvc.Get<JenkinsLastTestReports>(url.ToString(), x => x.Headers.Authorization = GetAuthHeader());
			}

			_statsGatherer.Collect(state, testReports);
		}

		private void Notify(State state)
		{
			Guard.IsNotNull(state, nameof(state));

			var message = string.Format(
				@"```
					╒═════════╤══════════╕
					│  STATS  │  Nº {0}  │
					╞═════════╧══════════╛
					│
					╞> Repository: {1}
					╞> Branch's name: {2}
					│
					╞> Last state: {3}
					│
					╞> Took {4}
					╞> Test failures: {5}
					│
					╘> (Re-)scheduled: {6}
				```",
				string.Format("{0:000}", state.Revision), //< 0
				state.Schedule.Repository, //< 1
				state.Schedule.Branch, //< 2
				state.LastState, //< 3,
				state.LastDuration.Prettify(), //< 4
				state.TestFailures?.ToString() ?? "N/A", //< 5
				state.IsDone ? "No" : "Yes" //< 6
			);

			_telegram.Send(message);
		}

		private void RebuildFor(State state)
		{
			Guard.IsNotNull(state, nameof(state));

			var url = new Uri(_settings.Url);
			var branch = state.Schedule.Branch.Replace("/", DSL_ENCODING);
			url = new Uri(url, string.Format(_settings.RebuildPath, state.Schedule.Repository, branch));

			_httpClientSvc.Send(url.ToString(), HttpMethod.Post, x => x.Headers.Authorization = GetAuthHeader());
		}

		private void TimerCallback(object obj)
		{
			var state = obj as State;
			Guard.IsNotNull(state, nameof(state));

			try
			{
				state.Timer.Change(Timeout.Infinite, Timeout.Infinite); //< Stop it!

				if (_cts.IsCancellationRequested)
					return;

				// Do HTTP/s request..
				var response = GetLastBuildFor(state);
				if (response.Building) //< If building just reschedule the timer.
				{
					_logger.LogDebug("Request made for {0}; a job is already running", state.Schedule);
					goto RESTART;
				}

				if (_cts.IsCancellationRequested)
					return;

				MayCollect(response, state);
				Dump(response, state); //< Load into the state the last results.
				Notify(state);

				if (state.IsDone) //< If success.. go on.
				{
					Remove(state.Schedule.Id);
					return;
				}

				RebuildFor(state);

			RESTART:
				state.Timer.Change(_settings.ScheduleDelay, _settings.ScheduleDelay);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Something went wrong, we'll remove schedule with uuid {0}", state.Schedule.Id);
				Remove(state.Schedule.Id);
			}
		}

		#endregion

		/// <summary>
		/// Check if there is a schedule w/ this identifier.
		/// </summary>
		public bool Exist(Guid scheduleId)
		{
			Guard.IsNotDefault(scheduleId, nameof(scheduleId));

			_logger.LogDebug("Checking if exists a schedule with uuid {0}", scheduleId);

			if (!_schedules.Keys.Any(x => x.Item1 == scheduleId))
			{
				_logger.LogInformation("Any schedule was found with uuid {0}", scheduleId);
				return false;
			}

			_logger.LogInformation("Schedule found for uuid {0}", scheduleId);

			return true;
		}

		/// <summary>
		/// Check if there is a schedule w/ this repository and branch.
		/// </summary>
		public bool Exist(string repository, string branch)
		{
			Guard.IsNotNullOrWhiteSpace(repository, nameof(repository));
			Guard.IsNotNullOrWhiteSpace(branch, nameof(branch));

			_logger.LogDebug("Checking if exists a schedule with repository {0}, branch {1}", repository, branch);

			if (!_schedules.Keys.Any(x => x.Item2 == branch && x.Item3 == repository))
			{
				_logger.LogInformation("Any schedule was found with repository {0}, branch {1}", repository, branch);
				return false;
			}

			_logger.LogInformation("Schedule found for repository {0}, branch {1}", repository, branch);

			return true;
		}

		/// <summary>
		/// Add a new schedule to the collection w/ a new timer.
		/// </summary>
		public Guid Create(string repository, string branch, bool metrics)
		{
			Guard.IsNotNullOrWhiteSpace(repository, nameof(repository));
			Guard.IsNotNullOrWhiteSpace(branch, nameof(branch));

			_logger.LogDebug("Creating schedule for repository {0}, branch {1}", repository, branch);

			var schedule = new JenkinsSchedule(branch, repository, metrics);
			var state = new State(schedule);

			state.Timer = new Timer(new TimerCallback(TimerCallback), state, dueTime: TimeSpan.Zero, period: _settings.ScheduleDelay);
			Guard.Against<InvalidOperationException>(!_schedules.TryAdd(schedule.Key, state), "Schedule {0} is already present?!", schedule);

			_logger.LogInformation("Created schedule for repository {0}, branch {1}", repository, branch);
			return schedule.Id;
		}

		/// <summary>
		/// If any schedule w/ this identifier is present.. we'll remove it disposing his timer.
		/// </summary>
		public void Remove(Guid scheduleId)
		{
			Guard.IsNotDefault(scheduleId, nameof(scheduleId));

			var schedule = _schedules.Keys.FirstOrDefault(x => x.Item1 == scheduleId);
			if (schedule == default)
			{
				_logger.LogInformation("Any schedule was found with uuid {0}.. skipping", scheduleId);
				return;
			}

			_logger.LogDebug("Removing schedule with uuid {0}", scheduleId);
			_schedules.TryRemove(schedule, out var state);
			state.Timer.Dispose();

			_logger.LogInformation("Removed schedule with uuid {0}", scheduleId);
		}

		public void Dispose()
		{
			if (_cts.IsCancellationRequested) return;
			_cts.Cancel();

			foreach (var schedule in _schedules)
				schedule.Value.Timer.Dispose();

			GC.SuppressFinalize(this);
		}
	}
}
