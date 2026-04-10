using AMFINAV.SchemeAPI.Domain.Common;
using AMFINAV.SchemeAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.SchemeAPI.Application.UseCases.Commands
{
    public class UpdateFundApprovalCommand
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateFundApprovalCommand> _logger;

        public UpdateFundApprovalCommand(IUnitOfWork unitOfWork,
            ILogger<UpdateFundApprovalCommand> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> ExecuteAsync(string fundCode, bool isApproved)
        {
            try
            {
                // Step 1 — Check fund exists in SchemeEnrollment
                var schemes = await _unitOfWork.SchemeEnrollments
                    .GetByFundCodeAsync(fundCode);

                var schemeList = schemes.ToList();

                if (schemeList.Count == 0)
                    return Result<int>.Failure(
                        $"No schemes found for FundCode '{fundCode}'.");

                _logger.LogInformation(
                    "Updating approval to {IsApproved} for FundCode={FundCode} " +
                    "— {Count} schemes affected",
                    isApproved, fundCode, schemeList.Count);

                // Step 2 — Update SchemeEnrollment table
                await _unitOfWork.SchemeEnrollments
                    .UpdateApprovalByFundCodeAsync(fundCode, isApproved);

                // Step 3 — Update DetailedScheme table
                await _unitOfWork.DetailedSchemes
                    .UpdateApprovalByFundCodeAsync(fundCode, isApproved);

                // Step 4 — Save both updates in one transaction
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "✅ Fund approval updated — FundCode={FundCode} IsApproved={IsApproved} " +
                    "Schemes affected={Count}",
                    fundCode, isApproved, schemeList.Count);

                return Result<int>.Success(schemeList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating fund approval for FundCode={FundCode}", fundCode);
                return Result<int>.Failure(ex.Message);
            }
        }
    }
}