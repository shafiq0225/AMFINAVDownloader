using MediatR;
using Microsoft.EntityFrameworkCore;
using AMFINAV.Identity.Application.Commands;
using AMFINAV.Identity.Application.DTOs;
using AMFINAV.Identity.Infrastructure.Data;

namespace AMFINAV.Identity.Application.Handlers;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, UserResponse>
{
    private readonly IdentityDbContext _context;

    public ActivateUserCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<UserResponse> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.UserId} not found");
        }

        user.IsActive = true;
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
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            FamilyHeadId = user.FamilyHeadId,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}