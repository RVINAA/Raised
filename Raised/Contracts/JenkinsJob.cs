namespace Raised.Contracts
{
	internal sealed class JenkinsJob : IEquatable<JenkinsJob>, IHasToken
	{
		#region Inner Types

		public enum State
		{
			Unknown = 0,
			Unstable,
			Success,
			Failure,
			Aborted
		}

		#endregion

		#region Properties

		public string B64 { get; set; }
		public string Url { get; set; }
		public int Duration { get; set; }
		public string Branch { get; set; }
		public State LastState { get; set; }
		public string Repository { get; set; }
		public int? TestFailures { get; set; }

		public bool? Cancel { get; set; }

		public bool IsSuccess => LastState == State.Success;
		public bool IsUnstable => LastState == State.Unstable;

		#endregion

		#region IHasToken members

		public string ApiToken { get; set; }
		public string Id { get; set; }

		#endregion

		public bool Equals(JenkinsJob other)
		{
			if (other == null)
				return false;

			return Repository == other.Repository && Branch == other.Branch;
		}

		public override bool Equals(object obj) => Equals(obj as JenkinsJob);

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17; //< Joshua Bloch's approach.
				hash *= 61 + Repository.GetHashCode();
				hash *= 61 + Branch.GetHashCode();

				return hash;
			}
		}
	}
}
