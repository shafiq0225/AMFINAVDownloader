using Microsoft.AspNetCore.Mvc;
using AMFINAV.SchemeAPI.Application.UseCases.Queries;

namespace AMFINAV.SchemeAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NavComparisonController : ControllerBase
    {
        private readonly GetNavComparisonQuery _query;

        public NavComparisonController(GetNavComparisonQuery query)
        {
            _query = query;
        }

        /// <summary>
        /// Custom date range comparison.
        /// GET /api/navcomparison?startDate=2026-04-08&endDate=2026-04-10
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetComparison(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
                return BadRequest("startDate must be earlier than endDate.");

            var result = await _query.ExecuteAsync(
                startDate.Date, endDate.Date);

            return result.IsSuccess
                ? Ok(result.Data)
                : NotFound(result.ErrorMessage);
        }

        /// <summary>
        /// Auto-detects last 2 trading days with actual data.
        /// Handles weekends and holidays automatically.
        /// GET /api/navcomparison/daily
        /// </summary>
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyComparison()
        {
            var result = await _query.ExecuteDailyAsync();

            return result.IsSuccess
                ? Ok(result.Data)
                : NotFound(result.ErrorMessage);
        }
    }
}