using Microsoft.AspNetCore.Mvc;
using AMFINAV.SchemeAPI.Application.UseCases.Commands;

namespace AMFINAV.SchemeAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FundApprovalController : ControllerBase
    {
        private readonly UpdateFundApprovalCommand _command;

        public FundApprovalController(UpdateFundApprovalCommand command)
        {
            _command = command;
        }

        /// <summary>
        /// Set approval for all schemes under a fund.
        /// PUT /api/fundapproval/{fundCode}?isApproved=true
        /// </summary>
        [HttpPut("{fundCode}")]
        public async Task<IActionResult> UpdateFundApproval(
            string fundCode, [FromQuery] bool isApproved)
        {
            if (string.IsNullOrWhiteSpace(fundCode))
                return BadRequest("FundCode is required.");

            var result = await _command.ExecuteAsync(fundCode, isApproved);

            return result.IsSuccess
                ? Ok(new
                {
                    FundCode = fundCode,
                    IsApproved = isApproved,
                    SchemesAffected = result.Data,
                    Message = $"Successfully updated {result.Data} scheme(s)"
                })
                : NotFound(result.ErrorMessage);
        }
    }
}