namespace AMFINAV.AuthAPI.Application.DTOs.Auth
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }
}