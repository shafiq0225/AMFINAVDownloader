using MediatR;
using AMFINAV.Identity.Application.DTOs;

namespace AMFINAV.Identity.Application.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserResponse>;