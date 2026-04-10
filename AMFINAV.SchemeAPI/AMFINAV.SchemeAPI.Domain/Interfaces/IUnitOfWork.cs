namespace AMFINAV.SchemeAPI.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ISchemeEnrollmentRepository SchemeEnrollments { get; }
        IDetailedSchemeRepository DetailedSchemes { get; }
        Task<int> CompleteAsync();
    }
}