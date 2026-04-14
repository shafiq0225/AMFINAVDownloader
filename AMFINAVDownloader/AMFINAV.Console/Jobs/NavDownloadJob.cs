using Microsoft.Extensions.Logging;
using Quartz;
using AMFINAV.Application.UseCases.Commands;
using AMFINAV.Infrastructure.Helpers;
using AMFINAV.Console.Exceptions;

namespace AMFINAV.Console.Jobs
{
    [DisallowConcurrentExecution]
    public class NavDownloadJob : IJob
    {
        private readonly DownloadAndStoreNavCommand _downloadCommand;
        private readonly ILogger<NavDownloadJob> _logger;

        public NavDownloadJob(DownloadAndStoreNavCommand downloadCommand, ILogger<NavDownloadJob> logger)
        {
            _downloadCommand = downloadCommand;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var startTime = DateTime.Now;
            _logger.LogInformation("========== Job Started at {Time} ==========", startTime);

            try
            {
                var targetDate = DateHelper.GetTargetNavDate();
                _logger.LogInformation("Target NAV date: {Date}", targetDate.ToString("yyyy-MM-dd"));

                var result = await _downloadCommand.ExecuteAsync(targetDate);

                if (result.IsSuccess)
                {
                    if (result.Data)
                        _logger.LogInformation("✅ Data downloaded and stored successfully");
                    else
                        _logger.LogInformation(
                            "Data already exists for {Date}",
                            targetDate.ToString("yyyy-MM-dd"));
                }
                else
                {
                    _logger.LogError(
                        "❌ Command failed: {Error}", result.ErrorMessage);

                    throw new AmfiNavException(
                        result.ErrorMessage ?? "Unknown error",
                        "JOB_COMMAND_FAILED");
                }
            }
            catch (NavDownloadException ex)
            {
                _logger.LogError(ex,
                    "❌ Download failed on attempt {Attempt} — ErrorCode: {Code}",
                    ex.AttemptNumber, ex.ErrorCode);

                WriteToEventLog(ex, System.Diagnostics.EventLogEntryType.Error);
                throw new JobExecutionException(ex, refireImmediately: false);
            }
            catch (NavStorageException ex)
            {
                _logger.LogError(ex,
                    "❌ Storage failed for NavDate {Date} — ErrorCode: {Code}",
                    ex.NavDate.ToString("yyyy-MM-dd"), ex.ErrorCode);

                WriteToEventLog(ex, System.Diagnostics.EventLogEntryType.Error);
                throw new JobExecutionException(ex, refireImmediately: false);
            }
            catch (NavPublishException ex)
            {
                _logger.LogError(ex,
                    "❌ Publish failed for NavDate {Date} — ErrorCode: {Code}",
                    ex.NavDate.ToString("yyyy-MM-dd"), ex.ErrorCode);

                // NAV was saved successfully — publish failure is non-critical
                // Log as warning, do not rethrow — job is partially successful
                WriteToEventLog(ex, System.Diagnostics.EventLogEntryType.Warning);
            }
            catch (AmfiNavException ex)
            {
                _logger.LogError(ex,
                    "❌ AMFINAV error — ErrorCode: {Code}, Message: {Message}",
                    ex.ErrorCode, ex.Message);

                WriteToEventLog(ex, System.Diagnostics.EventLogEntryType.Error);
                throw new JobExecutionException(ex, refireImmediately: false);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    "💥 Unexpected error in NavDownloadJob");

                WriteToEventLog(ex, System.Diagnostics.EventLogEntryType.Error);
                throw new JobExecutionException(ex, refireImmediately: false);
            }
            finally
            {
                var elapsed = DateTime.Now - startTime;
                _logger.LogInformation(
                    "========== Job Completed at {Time} — Elapsed: {Elapsed}s ==========",
                    DateTime.Now,
                    elapsed.TotalSeconds.ToString("F2"));
            }
        }

        private void WriteToEventLog(Exception ex,
            System.Diagnostics.EventLogEntryType type)
        {
            try
            {
                System.Diagnostics.EventLog.WriteEntry(
                    "AMFINAV NAV Downloader",
                    $"[{ex.GetType().Name}] {ex.Message}\n\n{ex.StackTrace}",
                    type);
            }
            catch
            {
                // Swallow — Event Log write failure should not crash the job
            }
        }
    }
}