using Raised.API.Facilities;

namespace Raised.API.Features
{
	internal class JenkinsScheduleState
	{
		#region Inner Types

		public enum _State
		{
			Unknown = 0,
			Unstable,
			Success,
			Failure,
			Aborted
		}

		#endregion

		#region Properties

		// Related to context.
		public JenkinsSchedule Schedule { get; private init; }
		public Timer Timer { get; set; }

		// Related to request.
		public _State LastState { get; set; }

		public int  Revision { get; set; }
		public int  LastDuration { get; set; }
		public int? TestFailures { get; set; }

		public bool IsUnstable => LastState == _State.Unstable;
		public bool IsSuccess => LastState == _State.Success;
		public bool IsDone => IsUnstable || IsSuccess;

		#endregion

		#region .ctors

		public JenkinsScheduleState(JenkinsSchedule schedule)
		{
			Schedule = schedule.ThrowIfNull(nameof(schedule));
		}

		#endregion
	}
}
