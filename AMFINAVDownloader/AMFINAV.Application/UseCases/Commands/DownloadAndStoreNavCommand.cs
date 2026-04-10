using System.Security.Cryptography;
using System.Text;
using AMFINAV.Domain.Common;
using AMFINAV.Domain.Entities;
using AMFINAV.Domain.Interfaces;
using AMFINAV.Domain.Enums;
using Microsoft.Extensions.Logging;
using MassTransit;
using AMFINAV.Domain.Contracts;

namespace AMFINAV.Application.UseCases.Commands
{
    public class DownloadAndStoreNavCommand
    {
        private readonly INavDownloadService _downloadService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DownloadAndStoreNavCommand> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
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

                // Check if already exists
                if (await _unitOfWork.NavFiles.ExistsByDateAsync(targetDate))
                {
                    _logger.LogInformation("Data already exists for {Date}", targetDate.ToString("yyyy-MM-dd"));
                    return Result<bool>.Success(false);
                }

                _logger.LogInformation("Downloading NAV data for {Date}", targetDate.ToString("yyyy-MM-dd"));

                // Download data
                var (status, content, errorMessage, recordCount) = await _downloadService.DownloadNavDataAsync();

                if (status != DownloadStatus.Success)
                {
                    _logger.LogError("Download failed: {Error}", errorMessage);
                    return Result<bool>.Failure(errorMessage);
                }

                // Store only the text file - no record parsing
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

                _logger.LogInformation("Successfully stored NAV text file for {Date}. Size: {Size} bytes, Records: {Records}",
                    targetDate.ToString("yyyy-MM-dd"), navFile.FileSizeBytes, recordCount);

                // ── Publish event to RabbitMQ ──────────────────────────────
                var navEvent = new NavFileProcessedEvent
                {
                    NavDate = targetDate,
                    FileContent = content,
                    RecordCount = recordCount,
                    PublishedAt = DateTime.UtcNow
                };

                await _publishEndpoint.Publish(navEvent);

                _logger.LogInformation(
                    "📤 Published NavFileProcessedEvent for {Date} with {Count} records",
                    targetDate.ToString("yyyy-MM-dd"), recordCount);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error executing download and store command");
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