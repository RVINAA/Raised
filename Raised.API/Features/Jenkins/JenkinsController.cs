using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

using Raised.API.Contracts;
using Raised.API.Facilities;

namespace Raised.API.Features
{
	[ApiController]
	[Route("api/jenkins")]
	public class JenkinsController : ControllerBase
	{
		#region Fields

		private readonly ILogger<JenkinsController> _logger;
		private readonly IJenkinsScheduleManager _scheduler;

		#endregion

		#region .ctors

		public JenkinsController(ILogger<JenkinsController> logger, IJenkinsScheduleManager scheduler)
		{
			_logger = logger.ThrowIfNull(nameof(logger));
			_scheduler = scheduler.ThrowIfNull(nameof(scheduler));
		}

		#endregion

		[HttpPost("schedules/add")]
		public IActionResult AddSchedule([Required]JenkinsScheduleAdd req)
		{
			_logger.LogDebug("Processing request to schedule a job for repository {0}, branch {1}", req.Repository, req.Branch);

			if (_scheduler.Exist(req.Repository, req.Branch))
			{
				_logger.LogInformation("Processed request to schedule a job; nothing to do");
				return Conflict();
			}

			var uuid = _scheduler.Create(req.Repository, req.Branch, req.Metrics);
			_logger.LogInformation("Processed request to schedule a job ({0}) for repository {1}, branch {2}", uuid, req.Repository, req.Branch);

			return Accepted(new JenkinsScheduleAdd._Result() { Id = uuid });
		}

		[HttpDelete("schedules/{uuid}")]
		public IActionResult DeleteSchedule([Required]Guid uuid)
		{
			_logger.LogDebug("Processing request to delete job scheduler with uuid {0}", uuid);

			if (!_scheduler.Exist(uuid))
			{
				_logger.LogInformation("Processed request to delete job scheduler; nothing to do");
				return NoContent();
			}

			_scheduler.Remove(uuid);
			_logger.LogInformation("Processed request to delete job scheduler with uuid {0}", uuid);

			return Accepted();
		}

		// GET schedules/stats
		// GET schedules/uuid/stats
	}
}
