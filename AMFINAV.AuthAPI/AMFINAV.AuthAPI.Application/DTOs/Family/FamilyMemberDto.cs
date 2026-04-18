namespace AMFINAV.AuthAPI.Application.DTOs.Family
{
    public class FamilyMemberDto
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PanNumber { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }
}