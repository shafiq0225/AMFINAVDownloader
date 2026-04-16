using Microsoft.AspNetCore.Mvc;
using AMFINAV.SchemeAPI.Application.UseCases.Queries;
using AMFINAV.SchemeAPI.Domain.Exceptions;

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

        [HttpGet]
        public async Task<IActionResult> GetComparison([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
                throw new ValidationException(new Dictionary<string, string[]>
        {
            { "dateRange", new[] { "startDate must be earlier than endDate." } }
        });

            var result = await _query.ExecuteAsync(startDate.Date, endDate.Date);
            return Ok(result);
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyComparison()
        {
            var result = await _query.ExecuteDailyAsync();
            return Ok(result);
        }
    }
}