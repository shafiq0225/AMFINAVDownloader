using AMFINAV.Investment.Application.Portfolio.Commands;
using Microsoft.AspNetCore.Mvc;

namespace AMFINAV.Investment.API.Controllers
{
    /// <summary>
    /// Allows manual triggering of background jobs.
    /// Used for testing and on-demand recalculation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly CalculateSnapshotCommand _snapshotCommand;
        private readonly ILogger<JobsController> _logger;

        public JobsController(
            CalculateSnapshotCommand snapshotCommand,
            ILogger<JobsController> logger)
        {
            _snapshotCommand = snapshotCommand;
            _logger = logger;
        }

        /// <summary>
        /// Manually trigger portfolio snapshot calculation.
        /// Useful for testing or recalculating after NAV update.
        /// </summary>
        [HttpPost("snapshot")]
        public async Task<IActionResult> TriggerSnapshot(
            [FromQuery] DateTime? date = null)
        {
            var targetDate = date?.Date ?? DateTime.Today;

            _logger.LogInformation(
                "Manual snapshot trigger for date: {Date}",
                targetDate.ToString("yyyy-MM-dd"));

            var result = await _snapshotCommand.ExecuteAsync(targetDate);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new
            {
                message = "Snapshot calculated successfully",
                snapshotDate = result.Data!.SnapshotDate
                    .ToString("yyyy-MM-dd"),
                totalHoldings = result.Data.TotalHoldings,
                calculated = result.Data.Calculated,
                skipped = result.Data.Skipped,
                noNavFound = result.Data.NoNavFound,
                totalInvested = result.Data.TotalInvested,
                totalValue = result.Data.TotalValue,
                totalProfitLoss = result.Data.TotalProfitLoss
            });
        }

        /// <summary>
        /// Health check — confirms job scheduler is running.
        /// </summary>
        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new
            {
                status = "Running",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                message = "Portfolio snapshot job is scheduled daily at 9:00 AM IST"
            });
        }
    }
}