using MediatR;
using AMFINAV.Identity.Application.DTOs;

namespace AMFINAV.Identity.Application.Commands;

public record RegisterCommand(RegisterRequest Request, Guid? CreatedBy = null) : IRequest<UserResponse>;