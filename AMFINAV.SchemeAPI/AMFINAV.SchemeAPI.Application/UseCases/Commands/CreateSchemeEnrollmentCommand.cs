using AMFINAV.SchemeAPI.Application.DTOs;
using AMFINAV.SchemeAPI.Domain.Common;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.SchemeAPI.Application.UseCases.Commands
{
    public class CreateSchemeEnrollmentCommand
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateSchemeEnrollmentCommand> _logger;

        public CreateSchemeEnrollmentCommand(IUnitOfWork unitOfWork, ILogger<CreateSchemeEnrollmentCommand> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<SchemeEnrollmentDto>> ExecuteAsync(CreateSchemeEnrollmentDto dto)
        {
            try
            {
                // Prevent duplicate SchemeCode
                if (await _unitOfWork.SchemeEnrollments.ExistsBySchemeCodeAsync(dto.SchemeCode))
                    return Result<SchemeEnrollmentDto>.Failure(
                        $"SchemeCode '{dto.SchemeCode}' is already enrolled.");

                var entity = new SchemeEnrollment
                {
                    FundCode = dto.FundCode.Trim(),
                    FundName = dto.FundName.Trim(),
                    SchemeCode = dto.SchemeCode.Trim(),
                    SchemeName = dto.SchemeName.Trim(),
                    IsApproved = dto.IsApproved,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.SchemeEnrollments.AddAsync(entity);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("SchemeEnrollment created for SchemeCode: {Code}", entity.SchemeCode);

                return Result<SchemeEnrollmentDto>.Success(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating SchemeEnrollment");
                return Result<SchemeEnrollmentDto>.Failure(ex.Message);
            }
        }

        private static SchemeEnrollmentDto MapToDto(SchemeEnrollment e) => new()
        {
            Id = e.Id,
            FundCode = e.FundCode,
            FundName = e.FundName,
            SchemeCode = e.SchemeCode,
            SchemeName = e.SchemeName,
            IsApproved = e.IsApproved,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}