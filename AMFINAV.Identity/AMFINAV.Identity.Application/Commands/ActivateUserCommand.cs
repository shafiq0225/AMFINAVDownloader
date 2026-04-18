using MediatR;
using AMFINAV.Identity.Application.DTOs;

namespace AMFINAV.Identity.Application.Commands;

public record ActivateUserCommand(Guid UserId) : IRequest<UserResponse>;