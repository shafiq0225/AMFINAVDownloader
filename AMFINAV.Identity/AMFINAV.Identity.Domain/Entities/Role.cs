using System.Security;

namespace AMFINAV.Identity.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Admin, Employee, User
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}