using AMFINAV.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMFINAV.Domain.Interfaces
{
    public interface IHolidayEventPublisher
    {
        Task PublishAsync(MarketHolidayEvent holidayEvent);
    }
}
