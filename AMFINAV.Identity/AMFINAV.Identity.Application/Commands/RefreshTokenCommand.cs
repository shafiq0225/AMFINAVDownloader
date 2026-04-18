using MediatR;
using AMFINAV.Identity.Domain.Contracts;

namespace AMFINAV.Identity.Application.Commands;

public record RefreshTokenCommand(string AccessToken, string RefreshToken, string IpAddress) : IRequest<TokenResponse>;