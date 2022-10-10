namespace Raised
{
	#region Interfaces

	internal interface IJenkinsJobWatcherSettings
	{
		/// <summary>
		/// Watcher's http listener url (with port).
		/// </summary>
		string EndPointPrefix { get; }
	}

	internal interface IJenkinsJobSchedulerSettings
	{
		/// <summary>
		/// Scheduler's timer delay (in ms).
		/// </summary>
		uint Interval { get; }
	}

	#endregion

	internal class JenkinsJobWatcherSettings : IJenkinsJobWatcherSettings
	{
		/// <summary>
		/// Watcher's http listener url (with port).
		/// </summary>
		public string EndPointPrefix { get; set; }
	}

	internal class JenkinsJobSchedulerSettings : IJenkinsJobSchedulerSettings
	{
		/// <summary>
		/// Scheduler's timer delay (in ms).
		/// </summary>
		public uint Interval { get; set; }
	}
}
