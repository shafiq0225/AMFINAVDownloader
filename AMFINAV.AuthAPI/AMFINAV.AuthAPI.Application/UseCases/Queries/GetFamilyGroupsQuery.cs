using AMFINAV.AuthAPI.Application.DTOs.Family;
using AMFINAV.AuthAPI.Application.UseCases.Commands;
using AMFINAV.AuthAPI.Domain.Interfaces;

namespace AMFINAV.AuthAPI.Application.UseCases.Queries
{
    public class GetFamilyGroupsQuery
    {
        private readonly IFamilyService _familyService;

        public GetFamilyGroupsQuery(IFamilyService familyService)
        {
            _familyService = familyService;
        }

        public async Task<IEnumerable<FamilyGroupDto>> GetAllAsync()
        {
            var groups = await _familyService.GetAllFamilyGroupsAsync();
            return groups.Select(CreateFamilyGroupCommand.MapToDto);
        }

        public async Task<FamilyGroupDto> GetByIdAsync(int groupId)
        {
            var group = await _familyService.GetFamilyGroupAsync(groupId);
            return CreateFamilyGroupCommand.MapToDto(group);
        }
    }
}