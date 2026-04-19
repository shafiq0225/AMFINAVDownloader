using Microsoft.AspNetCore.Mvc;
using AMFINAV.SchemeAPI.Application.DTOs;
using AMFINAV.SchemeAPI.Application.UseCases.Commands;
using AMFINAV.SchemeAPI.Application.UseCases.Queries;
using Microsoft.AspNetCore.Authorization;

namespace AMFINAV.SchemeAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SchemeEnrollmentController : ControllerBase
    {
        private readonly CreateSchemeEnrollmentCommand _createCommand;
        private readonly UpdateSchemeEnrollmentCommand _updateCommand;
        private readonly GetSchemeEnrollmentsQuery _query;

        public SchemeEnrollmentController(
            CreateSchemeEnrollmentCommand createCommand,
            UpdateSchemeEnrollmentCommand updateCommand,
            GetSchemeEnrollmentsQuery query)
        {
            _createCommand = createCommand;
            _updateCommand = updateCommand;
            _query = query;
        }

        [HttpGet]
        [Authorize(Policy = "CanReadSchemes")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _query.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{schemeCode}")]
        [Authorize(Policy = "CanReadSchemes")]
        public async Task<IActionResult> GetBySchemeCode(string schemeCode)
        {
            var result = await _query.GetBySchemeCodeAsync(schemeCode);
            return Ok(result);
        }

        [HttpGet("approved")]
        [Authorize(Policy = "CanReadSchemes")]
        public async Task<IActionResult> GetApproved()
        {
            var result = await _query.GetApprovedAsync();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "CanCreateSchemes")]
        public async Task<IActionResult> Create([FromBody] CreateSchemeEnrollmentDto dto)
        {
            var result = await _createCommand.ExecuteAsync(dto);
            return CreatedAtAction(nameof(GetBySchemeCode),
                new { schemeCode = result.SchemeCode }, result);
        }

        [HttpPut("{schemeCode}")]
        [Authorize(Policy = "CanUpdateSchemes")]
        public async Task<IActionResult> Update(string schemeCode,
            [FromBody] UpdateSchemeEnrollmentDto dto)
        {
            var result = await _updateCommand.ExecuteAsync(schemeCode, dto);
            return Ok(result);
        }
    }
}