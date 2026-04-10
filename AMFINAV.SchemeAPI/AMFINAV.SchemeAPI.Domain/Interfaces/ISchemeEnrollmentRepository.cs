using AMFINAV.SchemeAPI.Domain.Entities;

namespace AMFINAV.SchemeAPI.Domain.Interfaces
{
    public interface ISchemeEnrollmentRepository
    {
        Task<IEnumerable<SchemeEnrollment>> GetAllAsync();
        Task<SchemeEnrollment?> GetBySchemeCodeAsync(string schemeCode);
        Task<IEnumerable<SchemeEnrollment>> GetApprovedSchemesAsync();
        Task<IEnumerable<SchemeEnrollment>> GetByFundCodeAsync(string fundCode);
        Task<bool> ExistsBySchemeCodeAsync(string schemeCode);
        Task AddAsync(SchemeEnrollment scheme);
        Task UpdateAsync(string schemeCode, SchemeEnrollment scheme);
        Task UpdateApprovalByFundCodeAsync(string fundCode, bool isApproved);
    }
}