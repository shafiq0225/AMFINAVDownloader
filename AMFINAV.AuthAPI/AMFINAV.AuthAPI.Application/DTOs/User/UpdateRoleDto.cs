using AMFINAV.AuthAPI.Domain.Enums;

namespace AMFINAV.AuthAPI.Application.DTOs.User
{
    public class UpdateRoleDto
    {
        public UserRole NewRole { get; set; }
    }
}