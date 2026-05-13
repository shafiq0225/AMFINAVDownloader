using AMFINAV.Investment.Application.Orders.Dtos;
using AMFINAV.Investment.Domain.Common;
using AMFINAV.Investment.Domain.Enums;
using AMFINAV.Investment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.Investment.Application.Orders.Queries
{
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

        // ← Single optional filter parameter
        public async Task<Result<IEnumerable<InvestmentOrderDto>>> ExecuteAsync(
            string? investorIdFilter = null,
            string? statusFilter = null)
        {
            try
            {
                IEnumerable<Domain.Entities.InvestmentOrder> orders;

                if (!string.IsNullOrWhiteSpace(investorIdFilter))
                {
                    orders = await _unitOfWork.Orders
                        .GetByInvestorAsync(investorIdFilter);
                }
                else if (!string.IsNullOrWhiteSpace(statusFilter) &&
                         Enum.TryParse<OrderStatus>(
                             statusFilter, true, out var status))
                {
                    orders = await _unitOfWork.Orders
                        .GetByStatusAsync(status);
                }
                else
                {
                    orders = await _unitOfWork.Orders.GetAllAsync();
                }

                return Result<IEnumerable<InvestmentOrderDto>>
                    .Success(OrderMapper.ToDtoList(orders));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders");
                return Result<IEnumerable<InvestmentOrderDto>>
                    .Failure($"Failed to fetch orders: {ex.Message}");
            }
        }
    }
}