namespace AMFINAV.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        INavFileRepository NavFiles { get; }
        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}