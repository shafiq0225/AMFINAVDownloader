using MediatR;
using Microsoft.EntityFrameworkCore;
using AMFINAV.Identity.Application.Commands;
using AMFINAV.Identity.Application.DTOs;
using AMFINAV.Identity.Domain.Entities;
using AMFINAV.Identity.Infrastructure.Data;

namespace AMFINAV.Identity.Application.Handlers;

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, UserResponse>
{
    private readonly IdentityDbContext _context;

    public AssignRoleCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<UserResponse> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");
        }

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName, cancellationToken);

        if (role == null)
        {
            throw new KeyNotFoundException($"Role '{request.RoleName}' not found");
        }

        // Check if user already has role
        if (user.UserRoles.Any(ur => ur.Role.Name == request.RoleName))
        {
            throw new InvalidOperationException($"User already has role '{request.RoleName}'");
        }

        // Assign role
        _context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        // Refresh user data
        user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        return new UserResponse
        {
            Id = user!.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            FamilyHeadId = user.FamilyHeadId,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}