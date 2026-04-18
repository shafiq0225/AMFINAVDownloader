namespace AMFINAV.Identity.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }
    public string Resource { get; set; } = string.Empty;  // e.g., "SchemeEnrollment", "NAVComparison"
    public string Action { get; set; } = string.Empty;    // e.g., "Read", "Write", "Delete", "Approve"
    public string? Description { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}