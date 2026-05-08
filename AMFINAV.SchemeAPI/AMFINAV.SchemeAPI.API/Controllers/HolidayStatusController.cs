using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AMFINAV.SchemeAPI.Application.UseCases.Queries;

namespace AMFINAV.SchemeAPI.API.Controllers
{
    [ApiController]
    [Route("api/holiday-status")]
    [Authorize]
    public class HolidayStatusController : ControllerBase
    {
        private readonly GetHolidayStatusQuery _query;

        public HolidayStatusController(GetHolidayStatusQuery query)
        {
            _query = query;
        }

        /// <summary>
        /// Returns whether today is a market holiday.
        /// Angular uses this to show a banner when IsHoliday = true.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTodayStatus()
        {
            var result = await _query.ExecuteAsync();
            return Ok(result);
        }
    }
}