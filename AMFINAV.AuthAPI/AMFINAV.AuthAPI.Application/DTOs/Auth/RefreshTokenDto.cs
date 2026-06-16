using System.ComponentModel.DataAnnotations;

namespace AMFINAV.AuthAPI.Application.DTOs.Auth
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}