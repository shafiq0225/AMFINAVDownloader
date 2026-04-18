using MediatR;
using Microsoft.EntityFrameworkCore;
using AMFINAV.Identity.Application.Commands;
using AMFINAV.Identity.Domain.Contracts;
using AMFINAV.Identity.Infrastructure.Data;
using AMFINAV.Identity.Infrastructure.Services;

namespace AMFINAV.Identity.Application.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly IdentityDbContext _context;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IdentityDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Validate refresh token
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow || refreshToken.IsRevoked)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var user = refreshToken.User;

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is deactivated");
        }

        // Revoke old refresh token
        refreshToken.IsRevoked = true;

        // Get user roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role).ToList();
        var permissions = roles
            .SelectMany(r => r.Permissions)
            .GroupBy(p => p.Resource)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Action).ToList());

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _jwtService.CreateRefreshToken(user.Id, request.IpAddress);

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };
    }
}