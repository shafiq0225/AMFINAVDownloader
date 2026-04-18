using AMFINAV.AuthAPI.Application.DTOs.Family;
using AMFINAV.AuthAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.AuthAPI.Application.UseCases.Commands
{
    public class AddFamilyMemberCommand
    {
        private readonly IFamilyService _familyService;
        private readonly ILogger<AddFamilyMemberCommand> _logger;

        public AddFamilyMemberCommand(
            IFamilyService familyService,
            ILogger<AddFamilyMemberCommand> logger)
        {
            _familyService = familyService;
            _logger = logger;
        }

        public async Task<FamilyGroupDto> ExecuteAsync(
            int groupId, string userId, string adminId)
        {
            var group = await _familyService.AddMemberAsync(
                groupId, userId, adminId);

            _logger.LogInformation(
                "Family member added — GroupId={GroupId} " +
                "UserId={UserId} By={AdminId}",
                groupId, userId, adminId);

            return CreateFamilyGroupCommand.MapToDto(group);
        }
    }
}