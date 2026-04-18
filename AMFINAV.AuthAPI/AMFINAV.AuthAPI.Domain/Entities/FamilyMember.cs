namespace AMFINAV.AuthAPI.Domain.Entities
{
    /// <summary>
    /// Links a User (FamilyMember type) to a FamilyGroup.
    /// Only Admin can add or remove members.
    /// </summary>
    public class FamilyMember
    {
        public int Id { get; set; }
        public int FamilyGroupId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string AddedByAdminId { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ───────────────────────────────────────────────
        public FamilyGroup FamilyGroup { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}