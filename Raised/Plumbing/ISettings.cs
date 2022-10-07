namespace Raised
{
	internal interface IJenkinsJobWatcherSettings
	{
		string EndPointPrefix { get; }
	}

	internal class JenkinsJobWatcherSettings : IJenkinsJobWatcherSettings
	{
		public string EndPointPrefix { get; set; }
	}
}
