using Raised.API.Facilities;

namespace Raised.API.Features
{
	internal record JenkinsSchedule
	{
		public Guid Id { get; init; }
		public string Branch { get; private init; }
		public string Repository { get; private init; }

		internal (Guid, string, string) Key => (Id, Branch, Repository);

		public JenkinsSchedule(string branch, string repository)
		{
			Id = Guid.NewGuid();
			Branch = branch.ThrowIfNullOrWhiteSpace(nameof(branch));
			Repository = repository.ThrowIfNullOrWhiteSpace(nameof(repository));;
		}

		public override string ToString() => $"Repository: {Repository}, Branch: {Branch}";
	}
}
