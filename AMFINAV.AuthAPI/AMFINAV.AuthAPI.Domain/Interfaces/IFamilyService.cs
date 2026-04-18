using AMFINAV.AuthAPI.Domain.Entities;

namespace AMFINAV.AuthAPI.Domain.Interfaces
{
    public interface IFamilyService
    {
        Task<IEnumerable<FamilyGroup>> GetAllFamilyGroupsAsync();
        Task<FamilyGroup> GetFamilyGroupAsync(int groupId);

        /// <summary>
        /// Admin creates a family group and assigns a Head of Family.
        /// </summary>
        Task<FamilyGroup> CreateFamilyGroupAsync(
            string groupName,
            string headUserId,
            string adminId);

        /// <summary>
        /// Admin adds a User (FamilyMember) to a group.
        /// </summary>
        Task<FamilyGroup> AddMemberAsync(
            int groupId,
            string userId,
            string adminId);

        /// <summary>
        /// Admin removes a member from a group.
        /// </summary>
        Task RemoveMemberAsync(
            int groupId,
            string userId,
            string adminId);
    }
}