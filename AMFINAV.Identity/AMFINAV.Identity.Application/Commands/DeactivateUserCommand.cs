using MediatR;
using AMFINAV.Identity.Application.DTOs;

namespace AMFINAV.Identity.Application.Commands;

public record DeactivateUserCommand(Guid UserId) : IRequest<UserResponse>;