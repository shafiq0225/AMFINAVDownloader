using AMFINAV.SchemeAPI.Application.DTOs;
using AMFINAV.SchemeAPI.Domain.Entities;
using AMFINAV.SchemeAPI.Domain.Exceptions;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.SchemeAPI.Application.UseCases.Commands
{
    public class CreateSchemeEnrollmentCommand
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateSchemeEnrollmentCommand> _logger;

        public CreateSchemeEnrollmentCommand(IUnitOfWork unitOfWork,
            ILogger<CreateSchemeEnrollmentCommand> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SchemeEnrollmentDto> ExecuteAsync(
            CreateSchemeEnrollmentDto dto)
        {
            if (await _unitOfWork.SchemeEnrollments
                    .ExistsBySchemeCodeAsync(dto.SchemeCode))
                throw new DuplicateException("SchemeEnrollment", dto.SchemeCode);

            var entity = new SchemeEnrollment
            {
                SchemeCode = dto.SchemeCode.Trim(),
                SchemeName = dto.SchemeName.Trim(),
                IsApproved = dto.IsApproved,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SchemeEnrollments.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "SchemeEnrollment created — SchemeCode={Code} IsApproved={Approved}",
                entity.SchemeCode, entity.IsApproved);

            return MapToDto(entity);
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