using MediatR;
using Microsoft.EntityFrameworkCore;
using AMFINAV.Identity.Application.DTOs;
using AMFINAV.Identity.Application.Queries;
using AMFINAV.Identity.Infrastructure.Data;

namespace AMFINAV.Identity.Application.Handlers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserResponse>>
{
    private readonly IdentityDbContext _context;

    public GetAllUsersQueryHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync(cancellationToken);

        return users.Select(user => new UserResponse
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
        }).ToList();
    }
}