using AMFINAV.Investment.Domain.Common;
using AMFINAV.Investment.Domain.Entities;
using AMFINAV.Investment.Domain.Enums;
using AMFINAV.Investment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AMFINAV.Investment.Application.Orders.Commands
{
    // ── Request ───────────────────────────────────────────────────
    public class CreateOrderRequest
    {
        // Who is investing
        public string InvestorUserId { get; set; } = string.Empty;
        public string InvestorName { get; set; } = string.Empty;

        // Which scheme
        public string SchemeCode { get; set; } = string.Empty;
        public string SchemeName { get; set; } = string.Empty;
        public string FundName { get; set; } = string.Empty;

        // Amount
        public decimal InvestedAmount { get; set; }

        // Payment
        public PaymentMode PaymentMode { get; set; }
        public string? ChequeNumber { get; set; }
        public DateTime? ChequeDate { get; set; }
        public string? BankName { get; set; }
        public string? TransactionRef { get; set; }

        // Order date (defaults to today)
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // Optional notes
        public string? Notes { get; set; }

        // Admin creating this order
        public string CreatedByUserId { get; set; } = string.Empty;
    }

    // ── Command ───────────────────────────────────────────────────
    public class CreateOrderCommand
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateOrderCommand> _logger;

        public CreateOrderCommand(
            IUnitOfWork unitOfWork,
            ILogger<CreateOrderCommand> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<OrderDto>> ExecuteAsync(
            CreateOrderRequest request)
        {
            try
            {
                // ── Validate ───────────────────────────────────────
                var validation = Validate(request);
                if (!validation.IsSuccess)
                    return Result<OrderDto>.Failure(
                        validation.ErrorMessage!);

                // ── Generate order number ──────────────────────────
                var orderNumber = await _unitOfWork.Orders
                    .GenerateOrderNumberAsync();

                // ── Create entity ──────────────────────────────────
                var order = new InvestmentOrder
                {
                    OrderNumber = orderNumber,
                    InvestorUserId = request.InvestorUserId,
                    InvestorName = request.InvestorName,
                    SchemeCode = request.SchemeCode,
                    SchemeName = request.SchemeName,
                    FundName = request.FundName,
                    InvestedAmount = request.InvestedAmount,
                    PaymentMode = request.PaymentMode,
                    ChequeNumber = request.ChequeNumber,
                    ChequeDate = request.ChequeDate,
                    BankName = request.BankName,
                    TransactionRef = request.TransactionRef,
                    OrderDate = request.OrderDate.Date,
                    Status = OrderStatus.Pending,
                    Notes = request.Notes,
                    CreatedByUserId = request.CreatedByUserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // ── Save ───────────────────────────────────────────
                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "Investment order {OrderNumber} created for " +
                    "investor {InvestorName} — Scheme: {SchemeName} " +
                    "— Amount: {Amount}",
                    orderNumber,
                    request.InvestorName,
                    request.SchemeName,
                    request.InvestedAmount);

                return Result<OrderDto>.Success(MapToDto(order));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create investment order for {Investor}",
                    request.InvestorName);
                return Result<OrderDto>.Failure(
                    $"Failed to create order: {ex.Message}");
            }
        }

        // ── Validation ────────────────────────────────────────────
        private Result Validate(CreateOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.InvestorUserId))
                return Result.Failure("Investor is required.");

            if (string.IsNullOrWhiteSpace(request.SchemeCode))
                return Result.Failure("Scheme code is required.");

            if (string.IsNullOrWhiteSpace(request.SchemeName))
                return Result.Failure("Scheme name is required.");

            if (request.InvestedAmount <= 0)
                return Result.Failure(
                    "Invested amount must be greater than zero.");

            // Cheque-specific validation
            if (request.PaymentMode == PaymentMode.Cheque)
            {
                if (string.IsNullOrWhiteSpace(request.ChequeNumber))
                    return Result.Failure(
                        "Cheque number is required for cheque payment.");

                if (request.ChequeDate == null)
                    return Result.Failure(
                        "Cheque date is required for cheque payment.");

                if (string.IsNullOrWhiteSpace(request.BankName))
                    return Result.Failure(
                        "Bank name is required for cheque payment.");
            }

            // NEFT/RTGS validation
            if (request.PaymentMode == PaymentMode.NEFT ||
                request.PaymentMode == PaymentMode.RTGS ||
                request.PaymentMode == PaymentMode.IMPS)
            {
                if (string.IsNullOrWhiteSpace(request.TransactionRef))
                    return Result.Failure(
                        "Transaction reference is required " +
                        "for NEFT/RTGS/IMPS payment.");
            }

            if (string.IsNullOrWhiteSpace(request.CreatedByUserId))
                return Result.Failure("Created by user is required.");

            return Result.Success();
        }

        // ── Map Entity → DTO ──────────────────────────────────────
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