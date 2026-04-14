using System.Security.Cryptography;
using System.Text;
using AMFINAV.Domain.Common;
using AMFINAV.Domain.Contracts;
using AMFINAV.Domain.Entities;
using AMFINAV.Domain.Interfaces;
using AMFINAV.Domain.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace AMFINAV.Application.UseCases.Commands
{
    public class DownloadAndStoreNavCommand
    {
        private readonly INavDownloadService _downloadService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<DownloadAndStoreNavCommand> _logger;

        public DownloadAndStoreNavCommand(
            INavDownloadService downloadService,
            IUnitOfWork unitOfWork,
            IPublishEndpoint publishEndpoint,
            ILogger<DownloadAndStoreNavCommand> logger)
        {
            _downloadService = downloadService;
            _unitOfWork = unitOfWork;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<Result<bool>> ExecuteAsync(DateTime targetDate)
        {
            try
            {
                _logger.LogInformation("Checking if data exists for {Date}", targetDate.ToString("yyyy-MM-dd"));

                if (await _unitOfWork.NavFiles.ExistsByDateAsync(targetDate))
                {
                    _logger.LogInformation("Data already exists for {Date}", targetDate.ToString("yyyy-MM-dd"));
                    return Result<bool>.Success(false);
                }

                // ── Download ──────────────────────────────────────────────
                _logger.LogInformation("Downloading NAV data for {Date}", targetDate.ToString("yyyy-MM-dd"));

                var (status, content, errorMessage, recordCount) = await _downloadService.DownloadNavDataAsync();

                if (status != DownloadStatus.Success)
                {
                    _logger.LogError("Download failed: {Error}", errorMessage);
                    return Result<bool>.Failure(errorMessage);
                }

                // ── Store ─────────────────────────────────────────────────
                try
                {
                    await _unitOfWork.BeginTransactionAsync();

                    var navFile = new NavFile
                    {
                        NavDate = targetDate,
                        FileContent = content,
                        FileSizeBytes = Encoding.UTF8.GetByteCount(content),
                        RecordCount = recordCount,
                        Checksum = CalculateChecksum(content),
                        DownloadedAt = DateTime.Now
                    };

                    await _unitOfWork.NavFiles.AddAsync(navFile);
                    await _unitOfWork.CompleteAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation(
                        "Successfully stored NAV file for {Date}. " +
                        "Size: {Size} bytes, Records: {Records}",
                        targetDate.ToString("yyyy-MM-dd"),
                        navFile.FileSizeBytes, recordCount);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex,
                        "Failed to store NAV file for {Date}",
                        targetDate.ToString("yyyy-MM-dd"));
                    return Result<bool>.Failure(
                        $"Storage failed: {ex.Message}");
                }

                // ── Publish ───────────────────────────────────────────────
                try
                {
                    var navEvent = new NavFileProcessedEvent
                    {
                        NavDate = targetDate,
                        FileContent = content,
                        RecordCount = recordCount,
                        PublishedAt = DateTime.UtcNow
                    };

                    await _publishEndpoint.Publish(navEvent);

                    _logger.LogInformation("📤 Published NavFileProcessedEvent for {Date} with {Count} records", targetDate.ToString("yyyy-MM-dd"), recordCount);
                }
                catch (Exception ex)
                {
                    // Publish failure is non-critical — NAV data is already saved
                    // Log warning but return success
                    _logger.LogWarning(ex,
                        "⚠️ NAV saved but publish failed for {Date}. " +
                        "Consumer will miss this event.",
                        targetDate.ToString("yyyy-MM-dd"));
                }

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error in DownloadAndStoreNavCommand");
                return Result<bool>.Failure(ex.Message);
            }
        }

        private string CalculateChecksum(string content)
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}