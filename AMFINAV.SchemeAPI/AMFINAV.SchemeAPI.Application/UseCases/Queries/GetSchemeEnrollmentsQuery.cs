using AMFINAV.SchemeAPI.Application.DTOs;
using AMFINAV.SchemeAPI.Domain.Common;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.SchemeAPI.Application.UseCases.Queries
{
    public class GetSchemeEnrollmentsQuery
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetSchemeEnrollmentsQuery> _logger;

        public GetSchemeEnrollmentsQuery(IUnitOfWork unitOfWork,
            ILogger<GetSchemeEnrollmentsQuery> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<SchemeEnrollmentDto>>> GetAllAsync()
        {
            try
            {
                var list = await _unitOfWork.SchemeEnrollments.GetAllAsync();
                return Result<IEnumerable<SchemeEnrollmentDto>>.Success(list.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all SchemeEnrollments");
                return Result<IEnumerable<SchemeEnrollmentDto>>.Failure(ex.Message);
            }
        }

        // ← GetByIdAsync removed, replaced with GetBySchemeCodeAsync
        public async Task<Result<SchemeEnrollmentDto>> GetBySchemeCodeAsync(string schemeCode)
        {
            try
            {
                var entity = await _unitOfWork.SchemeEnrollments.GetBySchemeCodeAsync(schemeCode);
                if (entity is null)
                    return Result<SchemeEnrollmentDto>.Failure(
                        $"SchemeCode '{schemeCode}' not found.");

                return Result<SchemeEnrollmentDto>.Success(MapToDto(entity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving SchemeEnrollment SchemeCode={Code}", schemeCode);
                return Result<SchemeEnrollmentDto>.Failure(ex.Message);
            }
        }

        public async Task<Result<IEnumerable<SchemeEnrollmentDto>>> GetApprovedAsync()
        {
            try
            {
                var list = await _unitOfWork.SchemeEnrollments.GetApprovedSchemesAsync();
                return Result<IEnumerable<SchemeEnrollmentDto>>.Success(list.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approved SchemeEnrollments");
                return Result<IEnumerable<SchemeEnrollmentDto>>.Failure(ex.Message);
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