using Raised.API.Facilities;

namespace Raised.API.Features
{
	using State = JenkinsScheduleState;

	public class JenkinsScheduleStats
	{
		#region Properties

		public Guid ScheduleId { get; private init; }

		/// <summary>
		/// How many times this schedule has been executed.
		/// </summary>
		public int Executions { get; set; }

		/// <summary>
		/// Collection of durations of each execution.
		/// </summary>
		public IList<TimeSpan> Durations { get; } = new List<TimeSpan>();

		/// <summary>
		/// Grouping by test name, count how many times have been failing.
		/// </summary>
		public IDictionary<string, int> TestFailuresCount { get; } = new Dictionary<string, int>();

		/// <summary>
		/// How many times each result/outcome has been produced.
		/// </summary>
		public IDictionary<State._State, int> ResultsCount { get; } = ((State._State[])Enum.GetValues(typeof(State._State))).ToDictionary(x => x, x => 0);

		#endregion

		#region .ctors

		public JenkinsScheduleStats(Guid scheduleId)
		{
			Guard.IsNotDefault(scheduleId, nameof(scheduleId));
			ScheduleId = scheduleId;
		}

		#endregion
	}
}
