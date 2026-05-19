using Microsoft.AspNetCore.Mvc;
using AMFINAV.SchemeAPI.Application.UseCases.Queries;
using AMFINAV.SchemeAPI.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace AMFINAV.SchemeAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "CanReadNav")]
    public class NavComparisonController : ControllerBase
    {
        private readonly GetNavComparisonQuery _query;
        private readonly GetSchemeDetailsQuery _detailsQuery;

        public NavComparisonController(GetNavComparisonQuery query, GetSchemeDetailsQuery detailsQuery)
        {
            _query = query;
            _detailsQuery = detailsQuery;
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

        [HttpGet("{schemeCode}/details")]
        public async Task<IActionResult> GetSchemeDetails(string schemeCode)
        {
            if (string.IsNullOrWhiteSpace(schemeCode))
                return BadRequest(new { error = "Scheme code is required." });

            var result = await _detailsQuery.ExecuteAsync(schemeCode);
            return Ok(result);
        }
    }
}