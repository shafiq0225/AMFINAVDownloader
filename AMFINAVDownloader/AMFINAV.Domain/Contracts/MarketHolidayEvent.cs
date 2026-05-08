using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMFINAV.Domain.Contracts
{
    public record MarketHolidayEvent
    {
        public DateTime HolidayDate { get; init; }
        public string Source { get; init; } = "AMFINAV-App";
        public DateTime PublishedAt { get; init; }
    }

}
