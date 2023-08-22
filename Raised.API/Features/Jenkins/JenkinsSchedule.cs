using Raised.API.Facilities;

namespace Raised.API.Features
{
	public record JenkinsSchedule
	{
		public Guid Id { get; } = Guid.NewGuid();

		public bool Collect {get; private init; }
		public string Branch { get; private init; }
		public string Repository { get; private init; }

		internal (Guid, string, string) Key => (Id, Branch, Repository);

		public JenkinsSchedule(string branch, string repository, bool metrics)
		{
			Collect = metrics;
			Branch = branch.ThrowIfNullOrWhiteSpace(nameof(branch));
			Repository = repository.ThrowIfNullOrWhiteSpace(nameof(repository));;
		}

		public override string ToString() => $"Repository: {Repository}, Branch: {Branch}";
	}
}
