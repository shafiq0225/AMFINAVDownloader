using MediatR;
using Microsoft.EntityFrameworkCore;
using AMFINAV.Identity.Application.Commands;
using AMFINAV.Identity.Domain.Contracts;
using AMFINAV.Identity.Domain.Entities;
using AMFINAV.Identity.Infrastructure.Data;
using AMFINAV.Identity.Infrastructure.Services;

namespace AMFINAV.Identity.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenResponse>
{
    private readonly IdentityDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IdentityDbContext context, IPasswordService passwordService, IJwtService jwtService)
    {
        _context = context;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email or username
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == request.Request.UsernameOrEmail ||
                                      u.Username == request.Request.UsernameOrEmail, cancellationToken);

        if (user == null || !_passwordService.VerifyPassword(request.Request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username/email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is deactivated");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Get user roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role).ToList();
        var permissions = roles
            .SelectMany(r => r.Permissions)
            .GroupBy(p => p.Resource)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Action).ToList());

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtService.CreateRefreshToken(user.Id, request.IpAddress);

        // Store refresh token
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };
    }
}