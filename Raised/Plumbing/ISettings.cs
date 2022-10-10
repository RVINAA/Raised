namespace Raised
{
	#region Interfaces

	internal interface IJenkinsJobWatcherSettings
	{
		string EndPointPrefix { get; }
	}

	internal interface IJenkinsJobSchedulerSettings
	{
		uint Interval { get; }
	}

	#endregion

	internal class JenkinsJobWatcherSettings : IJenkinsJobWatcherSettings
	{
		public string EndPointPrefix { get; set; }
	}

	internal class JenkinsJobSchedulerSettings : IJenkinsJobSchedulerSettings
	{
		public uint Interval { get; set; }
	}
}
