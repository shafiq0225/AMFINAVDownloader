using AMFINAV.SchemeAPI.Domain.Entities;

namespace AMFINAV.SchemeAPI.Domain.Interfaces
{
    public interface IDetailedSchemeRepository
    {
        Task<bool> ExistsBySchemeCodeAndDateAsync(string schemeCode, DateTime navDate);
        Task AddRangeAsync(IEnumerable<DetailedScheme> schemes);
        Task UpdateApprovalByFundCodeAsync(string fundCode, bool isApproved);
        Task<IEnumerable<string>> GetSchemeCodesByFundCodeAsync(string fundCode);
    }
}