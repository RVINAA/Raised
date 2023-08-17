using System.ComponentModel.DataAnnotations;

namespace Raised.API.Contracts
{
	public class JenkinsScheduleAdd
	{
		#region Inner Types

		public class _Result
		{
			public Guid Id { get; set; }
		}

		#endregion

		/// <summary>
		/// Branch's name.
		/// </summary>
		[Required]
		public string Branch { get; set; }

		/// <summary>
		/// Repository's name (where branch is located).
		/// </summary>
		[Required]
		public string Repository { get; set; }
	}
}