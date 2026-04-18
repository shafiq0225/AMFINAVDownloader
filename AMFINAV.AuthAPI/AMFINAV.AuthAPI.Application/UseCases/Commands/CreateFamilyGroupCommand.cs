using AMFINAV.AuthAPI.Application.DTOs.Family;
using AMFINAV.AuthAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.AuthAPI.Application.UseCases.Commands
{
    public class CreateFamilyGroupCommand
    {
        private readonly IFamilyService _familyService;
        private readonly ILogger<CreateFamilyGroupCommand> _logger;

        public CreateFamilyGroupCommand(
            IFamilyService familyService,
            ILogger<CreateFamilyGroupCommand> logger)
        {
            _familyService = familyService;
            _logger = logger;
        }

        public async Task<FamilyGroupDto> ExecuteAsync(
            CreateFamilyGroupDto dto, string adminId)
        {
            var group = await _familyService.CreateFamilyGroupAsync(
                dto.GroupName, dto.HeadUserId, adminId);

            _logger.LogInformation(
                "Family group created — GroupId={Id} " +
                "Head={HeadUserId} By={AdminId}",
                group.Id, dto.HeadUserId, adminId);

            return MapToDto(group);
        }

        internal static FamilyGroupDto MapToDto(
            Domain.Entities.FamilyGroup g) => new()
            {
                Id = g.Id,
                GroupName = g.GroupName,
                HeadUserId = g.HeadUserId,
                HeadUserName = $"{g.HeadUser?.FirstName} {g.HeadUser?.LastName}",
                HeadUserEmail = g.HeadUser?.Email ?? string.Empty,
                HeadPanNumber = g.HeadUser?.PanNumber ?? string.Empty,
                CreatedAt = g.CreatedAt,
                IsActive = g.IsActive,
                Members = g.Members.Select(m => new FamilyMemberDto
                {
                    UserId = m.UserId,
                    FullName = $"{m.User?.FirstName} {m.User?.LastName}",
                    Email = m.User?.Email ?? string.Empty,
                    PanNumber = m.User?.PanNumber ?? string.Empty,
                    AddedAt = m.AddedAt
                }).ToList()
            };
    }
}