namespace AMFINAV.AuthAPI.Application.DTOs.Family
{
    public class FamilyGroupDto
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string HeadUserId { get; set; } = string.Empty;
        public string HeadUserName { get; set; } = string.Empty;
        public string HeadUserEmail { get; set; } = string.Empty;
        public string HeadPanNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public List<FamilyMemberDto> Members { get; set; } = new();
        public int MemberCount => Members.Count;
    }
}