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
                // Step 1 — Get all SchemeCodes under this FundCode from DetailedScheme
                var schemeCodes = await _unitOfWork.DetailedSchemes
                    .GetSchemeCodesByFundCodeAsync(fundCode);

                var schemeCodeList = schemeCodes.ToList();

                if (schemeCodeList.Count == 0)
                    return Result<int>.Failure(
                        $"No schemes found for FundCode '{fundCode}' " +
                        $"in DetailedScheme. Make sure NAV data has been " +
                        $"processed for this fund first.");

                _logger.LogInformation(
                    "Fund approval update — FundCode={FundCode} " +
                    "IsApproved={IsApproved} Schemes={Count}",
                    fundCode, isApproved, schemeCodeList.Count);

                // Step 2 — Update DetailedScheme (all rows for this FundCode)
                await _unitOfWork.DetailedSchemes
                    .UpdateApprovalByFundCodeAsync(fundCode, isApproved);

                // Step 3 — Update SchemeEnrollment (matching SchemeCodes)
                await _unitOfWork.SchemeEnrollments
                    .UpdateApprovalBySchemeCodesAsync(schemeCodeList, isApproved);

                // Step 4 — Save both in one transaction
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "✅ Fund approval updated — FundCode={FundCode} " +
                    "IsApproved={IsApproved} SchemesAffected={Count}",
                    fundCode, isApproved, schemeCodeList.Count);

                return Result<int>.Success(schemeCodeList.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating fund approval FundCode={FundCode}", fundCode);
                return Result<int>.Failure(ex.Message);
            }
        }
    }
}