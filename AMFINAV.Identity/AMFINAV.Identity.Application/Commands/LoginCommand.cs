using MediatR;
using AMFINAV.Identity.Domain.Contracts;
using AMFINAV.Identity.Application.DTOs;

namespace AMFINAV.Identity.Application.Commands;

public record LoginCommand(LoginRequest Request, string IpAddress) : IRequest<TokenResponse>;