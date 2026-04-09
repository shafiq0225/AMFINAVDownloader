using Microsoft.AspNetCore.Mvc;
using AMFINAV.SchemeAPI.Application.DTOs;
using AMFINAV.SchemeAPI.Application.UseCases.Commands;
using AMFINAV.SchemeAPI.Application.UseCases.Queries;

namespace AMFINAV.SchemeAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        // GET api/schemeenrollment
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _query.GetAllAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        // GET api/schemeenrollment/{schemeCode}   ← was /{id:int}
        [HttpGet("{schemeCode}")]
        public async Task<IActionResult> GetBySchemeCode(string schemeCode)
        {
            var result = await _query.GetBySchemeCodeAsync(schemeCode);
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }

        // GET api/schemeenrollment/approved
        [HttpGet("approved")]
        public async Task<IActionResult> GetApproved()
        {
            var result = await _query.GetApprovedAsync();
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        // POST api/schemeenrollment
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSchemeEnrollmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _createCommand.ExecuteAsync(dto);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetBySchemeCode),
                    new { schemeCode = result.Data!.SchemeCode }, result.Data)
                : BadRequest(result.ErrorMessage);
        }

        // PUT api/schemeenrollment/{schemeCode}   ← was /{id:int}
        [HttpPut("{schemeCode}")]
        public async Task<IActionResult> Update(string schemeCode,
            [FromBody] UpdateSchemeEnrollmentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _updateCommand.ExecuteAsync(schemeCode, dto);

            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }
    }
}