using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMFINAV.Domain.Interfaces
{
    public interface INseHolidayFetcher
    {
        Task<List<DateTime>> FetchHolidaysForYearAsync(int year);
        Task<HashSet<DateTime>> FetchAllHolidaysAsync();
        Task RefreshHolidaysAsync();
    }
}
