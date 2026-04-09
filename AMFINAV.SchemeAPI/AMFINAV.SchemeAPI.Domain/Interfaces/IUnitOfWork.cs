namespace AMFINAV.SchemeAPI.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ISchemeEnrollmentRepository SchemeEnrollments { get; }
        Task<int> CompleteAsync();
    }
}