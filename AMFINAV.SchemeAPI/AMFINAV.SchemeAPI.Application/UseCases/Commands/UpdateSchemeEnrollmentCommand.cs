using AMFINAV.SchemeAPI.Application.DTOs;
using AMFINAV.SchemeAPI.Domain.Common;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.SchemeAPI.Application.UseCases.Commands
{
    public class UpdateSchemeEnrollmentCommand
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateSchemeEnrollmentCommand> _logger;

        public UpdateSchemeEnrollmentCommand(IUnitOfWork unitOfWork,
            ILogger<UpdateSchemeEnrollmentCommand> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ← int id replaced with string schemeCode
        public async Task<Result<SchemeEnrollmentDto>> ExecuteAsync(
            string schemeCode, UpdateSchemeEnrollmentDto dto)
        {
            try
            {
                var existing = await _unitOfWork.SchemeEnrollments
                    .GetBySchemeCodeAsync(schemeCode);

                if (existing is null)
                    return Result<SchemeEnrollmentDto>.Failure(
                        $"SchemeCode '{schemeCode}' not found.");

                var updated = new SchemeEnrollment
                {
                    SchemeCode = existing.SchemeCode,
                    SchemeName = dto.SchemeName.Trim(),
                    IsApproved = dto.IsApproved,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.SchemeEnrollments.UpdateAsync(schemeCode, updated);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("SchemeEnrollment updated: SchemeCode={Code}", schemeCode);

                existing.SchemeName = updated.SchemeName;
                existing.IsApproved = updated.IsApproved;
                existing.UpdatedAt = updated.UpdatedAt;

                return Result<SchemeEnrollmentDto>.Success(MapToDto(existing));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating SchemeEnrollment SchemeCode={Code}", schemeCode);
                return Result<SchemeEnrollmentDto>.Failure(ex.Message);
            }
        }

        private static SchemeEnrollmentDto MapToDto(SchemeEnrollment e) => new()
        {
            Id = e.Id,
            SchemeCode = e.SchemeCode,
            SchemeName = e.SchemeName,
            IsApproved = e.IsApproved,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}