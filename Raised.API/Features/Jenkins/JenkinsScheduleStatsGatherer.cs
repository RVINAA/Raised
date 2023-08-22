using System.Collections.Concurrent;

using Raised.API.Facilities;

namespace Raised.API.Features
{
	using State = JenkinsScheduleState;
	using Stats = JenkinsScheduleStats;

	public interface IJenkinsScheduleStatsGatherer
	{
		void Collect(State state, JenkinsLastTestReports testReports);

		Stats Get(Guid uuid);
		Stats GetAll();
	}

	public class JenkinsScheduleStatsGatherer : IJenkinsScheduleStatsGatherer
	{
		#region Fields

		private readonly ConcurrentDictionary<Guid, Stats> _stats = new();
		private readonly ILogger<JenkinsScheduleStatsGatherer> _logger;

		#endregion

		#region .ctors

		public JenkinsScheduleStatsGatherer(ILogger<JenkinsScheduleStatsGatherer> logger)
		{
			_logger = logger.ThrowIfNull(nameof(logger));
		}

		#endregion

		public void Collect(State state, JenkinsLastTestReports testReports)
		{
			Guard.IsNotNull(state, nameof(state));

			_logger.LogDebug("Collecting stats for schedule with uuid {0}", state.Schedule.Id);

			var stats = _stats.GetOrAdd(state.Schedule.Id, uuid => new Stats(uuid));
			stats.Durations.Add(state.LastDuration);
			stats.ResultsCount[state.LastState]++;
			stats.Executions++;

			if (testReports != null)
			{

			}

			_logger.LogInformation("Collected stats for schedule with uuid {0}", state.Schedule.Id);
		}

		public Stats Get(Guid uuid)
		{
			Guard.IsNotDefault(uuid, nameof(uuid));

			_logger.LogInformation("Retrieving stats for schedule with uuid {0}", uuid);

			if (!_stats.TryGetValue(uuid, out var stats))
				throw new ArgumentException($"No stats found for identifier {uuid}");

			_logger.LogInformation("Retrieved stats for schedule with uuid {0}", uuid);

			return stats;
		}

		public Stats GetAll()
		{
			throw new NotImplementedException();
		}
	}
}
