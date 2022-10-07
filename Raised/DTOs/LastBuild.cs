using System.Text.Json.Serialization;

namespace Raised.DTOs
{
	internal record struct LastBuild
	{
		#region Inner Types

		public record struct Action
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
		/// Branch's name.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Revision number.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Job's name + Description + DisplayName.
		/// </summary>
		public string FullDisplayName { get; set; }

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

		public Action[] Actions { get; set; }

		#endregion
	}
}
