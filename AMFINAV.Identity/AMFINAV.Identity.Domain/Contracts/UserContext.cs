using AMFINAV.Identity.Domain.Enums;

namespace AMFINAV.Identity.Domain.Contracts;

public record UserContext
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public Dictionary<string, List<string>> Permissions { get; init; } = new();
    public UserType UserType { get; init; }
    public Guid? FamilyHeadId { get; init; }
}