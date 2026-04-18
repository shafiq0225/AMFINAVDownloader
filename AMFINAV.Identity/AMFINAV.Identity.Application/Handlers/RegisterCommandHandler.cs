using MediatR;
using Microsoft.EntityFrameworkCore;
using AMFINAV.Identity.Application.Commands;
using AMFINAV.Identity.Application.DTOs;
using AMFINAV.Identity.Domain.Entities;
using AMFINAV.Identity.Infrastructure.Data;
using AMFINAV.Identity.Infrastructure.Services;

namespace AMFINAV.Identity.Application.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UserResponse>
{
    private readonly IdentityDbContext _context;
    private readonly IPasswordService _passwordService;

    public RegisterCommandHandler(IdentityDbContext context, IPasswordService passwordService)
    {
        _context = context;
        _passwordService = passwordService;
    }

    public async Task<UserResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Request.Email || u.Username == request.Request.Username, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email or username already exists");
        }

        // Get role
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.Request.Role, cancellationToken);

        if (role == null)
        {
            throw new InvalidOperationException($"Role '{request.Request.Role}' does not exist");
        }

        // Verify family head if registering as family member
        if (request.Request.FamilyHeadId.HasValue)
        {
            var familyHead = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == request.Request.FamilyHeadId.Value, cancellationToken);

            if (familyHead == null || !familyHead.UserRoles.Any(ur => ur.Role.Name == "User"))
            {
                throw new InvalidOperationException("Invalid family head reference");
            }
        }

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Request.Email,
            Username = request.Request.Username,
            PasswordHash = _passwordService.HashPassword(request.Request.Password),
            FirstName = request.Request.FirstName,
            LastName = request.Request.LastName,
            PhoneNumber = request.Request.PhoneNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            FamilyHeadId = request.Request.FamilyHeadId
        };

        _context.Users.Add(user);

        // Assign role
        _context.UserRoles.Add(new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = request.CreatedBy
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            Roles = new List<string> { role.Name },
            FamilyHeadId = user.FamilyHeadId,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}