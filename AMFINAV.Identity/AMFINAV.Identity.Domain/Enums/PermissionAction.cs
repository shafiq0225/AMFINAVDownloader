namespace AMFINAV.Identity.Domain.Enums;

[Flags]
public enum PermissionAction
{
    None = 0,
    Read = 1,
    Create = 2,
    Update = 4,
    Delete = 8,
    Approve = 16,
    FullControl = Read | Create | Update | Delete | Approve
}