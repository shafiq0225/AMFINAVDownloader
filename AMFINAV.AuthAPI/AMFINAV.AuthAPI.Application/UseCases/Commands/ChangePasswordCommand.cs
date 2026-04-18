using AMFINAV.AuthAPI.Application.DTOs.Auth;
using AMFINAV.AuthAPI.Domain.Exceptions;
using AMFINAV.AuthAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.AuthAPI.Application.UseCases.Commands
{
    public class ChangePasswordCommand
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ChangePasswordCommand> _logger;

        public ChangePasswordCommand(
            IAuthService authService,
            ILogger<ChangePasswordCommand> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task ExecuteAsync(string userId, ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                throw new AuthException(
                    "New password and confirm password do not match.",
                    "PASSWORD_MISMATCH", 400);

            await _authService.ChangePasswordAsync(
                userId, dto.CurrentPassword, dto.NewPassword);

            _logger.LogInformation(
                "Password changed — UserId={UserId}", userId);
        }
    }
}