using AMFINAV.Investment.Domain.Common;
using AMFINAV.Investment.Domain.Enums;
using AMFINAV.Investment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.Investment.Application.Orders.Queries
{
    // ── Filter ────────────────────────────────────────────────────
    public class GetAllOrdersFilter
    {
        public OrderStatus? Status { get; set; }
        public string? InvestorName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    // ── Query ─────────────────────────────────────────────────────
    public class GetAllOrdersQuery
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllOrdersQuery> _logger;

        public GetAllOrdersQuery(
            IUnitOfWork unitOfWork,
            ILogger<GetAllOrdersQuery> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<OrderSummaryDto>>>
            ExecuteAsync(GetAllOrdersFilter? filter = null)
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetAllAsync();

                // ── Apply filters ──────────────────────────────────
                if (filter != null)
                {
                    if (filter.Status.HasValue)
                        orders = orders.Where(
                            o => o.Status == filter.Status.Value);

                    if (!string.IsNullOrWhiteSpace(filter.InvestorName))
                        orders = orders.Where(o =>
                            o.InvestorName.Contains(
                                filter.InvestorName,
                                StringComparison.OrdinalIgnoreCase));

                    if (filter.FromDate.HasValue)
                        orders = orders.Where(
                            o => o.OrderDate >= filter.FromDate.Value.Date);

                    if (filter.ToDate.HasValue)
                        orders = orders.Where(
                            o => o.OrderDate <= filter.ToDate.Value.Date);
                }

                var result = orders.Select(o => new OrderSummaryDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    InvestorName = o.InvestorName,
                    SchemeName = o.SchemeName,
                    FundName = o.FundName,
                    InvestedAmount = o.InvestedAmount,
                    PaymentMode = o.PaymentMode.ToString(),
                    Status = o.Status.ToString(),
                    StatusCode = (int)o.Status,
                    OrderDate = o.OrderDate,
                    HasStatement = o.Statement != null,
                    CreatedAt = o.CreatedAt
                });

                return Result<IEnumerable<OrderSummaryDto>>
                    .Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all orders");
                return Result<IEnumerable<OrderSummaryDto>>
                    .Failure($"Failed to retrieve orders: {ex.Message}");
            }
        }
    }
}