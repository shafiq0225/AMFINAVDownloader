using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AMFINAV.AuthAPI.Application.DTOs.Family;
using AMFINAV.AuthAPI.Application.UseCases.Commands;
using AMFINAV.AuthAPI.Application.UseCases.Queries;

namespace AMFINAV.AuthAPI.API.Controllers
{
    [ApiController]
    [Route("api/family")]
    [Authorize(Policy = "AdminOnly")]
    public class FamilyController : ControllerBase
    {
        private readonly GetFamilyGroupsQuery _query;
        private readonly CreateFamilyGroupCommand _createCommand;
        private readonly AddFamilyMemberCommand _addMemberCommand;
        private readonly RemoveFamilyMemberCommand _removeMemberCommand;

        public FamilyController(
            GetFamilyGroupsQuery query,
            CreateFamilyGroupCommand createCommand,
            AddFamilyMemberCommand addMemberCommand,
            RemoveFamilyMemberCommand removeMemberCommand)
        {
            _query = query;
            _createCommand = createCommand;
            _addMemberCommand = addMemberCommand;
            _removeMemberCommand = removeMemberCommand;
        }

        /// <summary>
        /// Get all family groups. Admin only.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var groups = await _query.GetAllAsync();
            return Ok(groups);
        }

        /// <summary>
        /// Get a specific family group by ID.
        /// </summary>
        [HttpGet("{groupId:int}")]
        public async Task<IActionResult> GetById(int groupId)
        {
            var group = await _query.GetByIdAsync(groupId);
            return Ok(group);
        }

        /// <summary>
        /// Create a new family group with a Head of Family. Admin only.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateFamilyGroupDto dto)
        {
            var adminId = GetCurrentUserId();
            var result = await _createCommand.ExecuteAsync(dto, adminId);
            return CreatedAtAction(
                nameof(GetById),
                new { groupId = result.Id },
                result);
        }

        /// <summary>
        /// Add a member to a family group. Admin only.
        /// </summary>
        [HttpPost("{groupId:int}/members")]
        public async Task<IActionResult> AddMember(
            int groupId, [FromBody] AddFamilyMemberDto dto)
        {
            var adminId = GetCurrentUserId();
            var result = await _addMemberCommand.ExecuteAsync(
                groupId, dto.UserId, adminId);
            return Ok(result);
        }

        /// <summary>
        /// Remove a member from a family group. Admin only.
        /// </summary>
        [HttpDelete("{groupId:int}/members/{userId}")]
        public async Task<IActionResult> RemoveMember(
            int groupId, string userId)
        {
            var adminId = GetCurrentUserId();
            await _removeMemberCommand.ExecuteAsync(
                groupId, userId, adminId);

            return Ok(new
            {
                message = $"User '{userId}' removed from " +
                          $"family group '{groupId}' successfully."
            });
        }

        private string GetCurrentUserId() =>
            User.FindFirstValue(
                System.IdentityModel.Tokens.Jwt
                    .JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(
                System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? string.Empty;
    }
}