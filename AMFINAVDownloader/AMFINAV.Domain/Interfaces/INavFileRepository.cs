using AMFINAV.Domain.Entities;

namespace AMFINAV.Domain.Interfaces
{
    public interface INavFileRepository
    {
        //Task<NavFile> GetByDateAsync(DateTime date);
        //Task<IEnumerable<DateTime>> GetAllDatesAsync();
        //Task<DateTime?> GetLatestDateAsync();
        Task<bool> ExistsByDateAsync(DateTime date);
        Task AddAsync(NavFile navFile);
    }
}