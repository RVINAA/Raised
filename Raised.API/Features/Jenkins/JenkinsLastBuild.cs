using System.Text.Json.Serialization;

namespace Raised.API.Features
{
	internal record JenkinsLastBuild
	{
		#region Inner Types

		public record _Action
		{
			#region Properties

			[JsonPropertyName("_class")]
			public string Class { get; set; }

			public int? FailCount { get; set; }

			#endregion
		}

		#endregion

		#region Properties

		/// <summary>
		/// Branch's name (ie. juanje/do-something).
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Revision number (ie. 23228.15115.0.17).
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Job's name + Description + DisplayName.
		/// </summary>
		public string FullDisplayName { get; set; }

		/// <summary>
		/// ie. 17 (from revision number).
		/// </summary>
		public int Number { get; set; }

		/// <summary>
		/// How long did it finally take (in ms). If not finished.. we'll get 0.
		/// </summary>
		public int Duration { get; set; }

		public bool Building { get; set; }

		/// <summary>
		/// If build finished, specifies the final result (else, NULL).
		///		Possible (known) values: (?)
		///		- ABORTED
		///		- SUCCESS
		///		- FAILURE
		///		- UNSTABLE
		/// </summary>
		public string Result { get; set; }

		public _Action[] Actions { get; set; }

		#endregion
	}
}
