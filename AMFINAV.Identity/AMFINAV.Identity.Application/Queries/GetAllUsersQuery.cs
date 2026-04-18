using MediatR;
using AMFINAV.Identity.Application.DTOs;

namespace AMFINAV.Identity.Application.Queries;

public record GetAllUsersQuery() : IRequest<List<UserResponse>>;