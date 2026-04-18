using MediatR;
using AMFINAV.Identity.Application.DTOs;

namespace AMFINAV.Identity.Application.Commands;

public record AssignRoleCommand(Guid UserId, string RoleName) : IRequest<UserResponse>;