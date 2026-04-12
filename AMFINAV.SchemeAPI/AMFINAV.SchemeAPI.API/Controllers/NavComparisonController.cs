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
        /// Compare NAV between two dates for all approved schemes.
        /// GET /api/navcomparison?startDate=2026-04-08&endDate=2026-04-10
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetComparison([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
                return BadRequest("startDate must be earlier than endDate.");

            // ← Normalize to UTC date only — strip time and timezone offset
            var normalizedStart = startDate.Date;
            var normalizedEnd = endDate.Date;

            var result = await _query.ExecuteAsync(normalizedStart, normalizedEnd);

            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }


        /// <summary>
        /// Compare yesterday vs day before yesterday (default daily comparison).
        /// GET /api/navcomparison/daily
        /// </summary>
        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyComparison()
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var dayBefore = today.AddDays(-2);

            var result = await _query.ExecuteAsync(dayBefore, yesterday);

            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }
    }
}