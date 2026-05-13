using AMFINAV.Investment.Domain.Common;
using AMFINAV.Investment.Domain.Entities;
using AMFINAV.Investment.Domain.Enums;
using AMFINAV.Investment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.Investment.Application.Orders.Commands
{
    // ── Request ───────────────────────────────────────────────────
    public class UpdateOrderStatusRequest
    {
        public int OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }

        // Required when moving to Submitted
        public DateTime? SubmittedDate { get; set; }
        public string? SubmittedByUserId { get; set; }

        // Required when moving to Confirmed
        // Admin enters these from MF company confirmation
        public decimal? PurchaseNAV { get; set; }
        public decimal? UnitsAllotted { get; set; }
        public string? FolioNumber { get; set; }
        public DateTime? ConfirmedDate { get; set; }

        // Optional
        public string? Notes { get; set; }
        public string UpdatedByUserId { get; set; } = string.Empty;
    }

    // ── Command ───────────────────────────────────────────────────
    public class UpdateOrderStatusCommand
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateOrderStatusCommand> _logger;

        public UpdateOrderStatusCommand(
            IUnitOfWork unitOfWork,
            ILogger<UpdateOrderStatusCommand> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<OrderDto>> ExecuteAsync(
            UpdateOrderStatusRequest request)
        {
            try
            {
                // ── Load order ─────────────────────────────────────
                var order = await _unitOfWork.Orders
                    .GetByIdAsync(request.OrderId);

                if (order == null)
                    return Result<OrderDto>.Failure(
                        $"Order {request.OrderId} not found.");

                // ── Validate transition ────────────────────────────
                var validation = ValidateTransition(
                    order.Status, request.NewStatus, request);
                if (!validation.IsSuccess)
                    return Result<OrderDto>.Failure(
                        validation.ErrorMessage!);

                // ── Apply status transition ────────────────────────
                await ApplyTransitionAsync(order, request);

                // ── Save ───────────────────────────────────────────
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "Order {OrderNumber} status updated: " +
                    "{OldStatus} → {NewStatus}",
                    order.OrderNumber,
                    order.Status,
                    request.NewStatus);

                return Result<OrderDto>.Success(MapToDto(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update order {OrderId} status",
                    request.OrderId);
                return Result<OrderDto>.Failure(
                    $"Failed to update status: {ex.Message}");
            }
        }

        // ── Validate Status Transition ────────────────────────────
        private Result ValidateTransition(
            OrderStatus current,
            OrderStatus next,
            UpdateOrderStatusRequest request)
        {
            // Define allowed transitions
            var allowed = new Dictionary<OrderStatus, OrderStatus[]>
            {
                { OrderStatus.Pending,
                    new[] { OrderStatus.Submitted,
                            OrderStatus.Cancelled } },

                { OrderStatus.Submitted,
                    new[] { OrderStatus.Confirmed,
                            OrderStatus.Cancelled } },

                { OrderStatus.Confirmed,
                    new[] { OrderStatus.Active } },

                { OrderStatus.Active,
                    new OrderStatus[] { } },  // No transitions from Active

                { OrderStatus.Cancelled,
                    new OrderStatus[] { } }   // No transitions from Cancelled
            };

            if (!allowed.ContainsKey(current) ||
                !allowed[current].Contains(next))
            {
                return Result.Failure(
                    $"Cannot change status from " +
                    $"{current} to {next}. " +
                    $"Allowed: {string.Join(", ", allowed[current])}");
            }

            // Submitted requires submission details
            if (next == OrderStatus.Submitted)
            {
                if (request.SubmittedDate == null)
                    return Result.Failure(
                        "Submitted date is required.");

                if (string.IsNullOrWhiteSpace(
                    request.SubmittedByUserId))
                    return Result.Failure(
                        "Submitted by user is required.");
            }

            // Confirmed requires MF company details
            if (next == OrderStatus.Confirmed)
            {
                if (request.PurchaseNAV == null || request.PurchaseNAV <= 0)
                    return Result.Failure(
                        "Purchase NAV is required for confirmation.");

                if (request.UnitsAllotted == null ||
                    request.UnitsAllotted <= 0)
                    return Result.Failure(
                        "Units allotted is required for confirmation.");

                if (string.IsNullOrWhiteSpace(request.FolioNumber))
                    return Result.Failure(
                        "Folio number is required for confirmation.");

                if (request.ConfirmedDate == null)
                    return Result.Failure(
                        "Confirmed date is required.");
            }

            return Result.Success();
        }

        // ── Apply Transition ──────────────────────────────────────
        private async Task ApplyTransitionAsync(
            InvestmentOrder order,
            UpdateOrderStatusRequest request)
        {
            var previousStatus = order.Status;
            order.Status = request.NewStatus;

            if (request.Notes != null)
                order.Notes = request.Notes;

            switch (request.NewStatus)
            {
                case OrderStatus.Submitted:
                    order.SubmittedDate = request.SubmittedDate;
                    order.SubmittedByUserId = request.SubmittedByUserId;
                    break;

                case OrderStatus.Confirmed:
                    order.ConfirmedDate = request.ConfirmedDate;
                    order.PurchaseNAV = request.PurchaseNAV;
                    order.UnitsAllotted = request.UnitsAllotted;
                    order.FolioNumber = request.FolioNumber;
                    break;

                case OrderStatus.Active:
                    // ── Create Holding automatically ───────────────
                    // When order moves to Active, create a Holding
                    // so daily P&L tracking starts
                    if (!await _unitOfWork.Holdings
                        .ExistsForOrderAsync(order.Id))
                    {
                        var holding = new Holding
                        {
                            OrderId = order.Id,
                            InvestorUserId = order.InvestorUserId,
                            InvestorName = order.InvestorName,
                            SchemeCode = order.SchemeCode,
                            SchemeName = order.SchemeName,
                            FundName = order.FundName,
                            FolioNumber = order.FolioNumber!,
                            PurchaseDate = order.ConfirmedDate!.Value,
                            PurchaseNAV = order.PurchaseNAV!.Value,
                            InvestedAmount = order.InvestedAmount,
                            Units = order.UnitsAllotted!.Value,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.Holdings.AddAsync(holding);

                        _logger.LogInformation(
                            "Holding created for order {OrderNumber} — " +
                            "Investor: {Investor}, Units: {Units}, " +
                            "Purchase NAV: {NAV}",
                            order.OrderNumber,
                            order.InvestorName,
                            holding.Units,
                            holding.PurchaseNAV);
                    }
                    break;
            }
        }

        // ── Map ───────────────────────────────────────────────────
        private static OrderDto MapToDto(InvestmentOrder o) => new()
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            InvestorUserId = o.InvestorUserId,
            InvestorName = o.InvestorName,
            SchemeCode = o.SchemeCode,
            SchemeName = o.SchemeName,
            FundName = o.FundName,
            InvestedAmount = o.InvestedAmount,
            PaymentMode = o.PaymentMode.ToString(),
            ChequeNumber = o.ChequeNumber,
            ChequeDate = o.ChequeDate,
            BankName = o.BankName,
            TransactionRef = o.TransactionRef,
            OrderDate = o.OrderDate,
            SubmittedDate = o.SubmittedDate,
            ConfirmedDate = o.ConfirmedDate,
            Status = o.Status.ToString(),
            StatusCode = (int)o.Status,
            PurchaseNAV = o.PurchaseNAV,
            UnitsAllotted = o.UnitsAllotted,
            FolioNumber = o.FolioNumber,
            Notes = o.Notes,
            CreatedByUserId = o.CreatedByUserId,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            HasHolding = o.Holding != null,
            HasStatement = o.Statement != null
        };
    }
}