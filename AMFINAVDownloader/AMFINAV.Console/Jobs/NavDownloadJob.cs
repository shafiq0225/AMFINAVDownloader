using Microsoft.Extensions.Logging;
using Quartz;
using AMFINAV.Application.UseCases.Commands;
using AMFINAV.Infrastructure.Helpers;

namespace AMFINAV.Console.Jobs
{
    [DisallowConcurrentExecution]
    public class NavDownloadJob : IJob
    {
        private readonly DownloadAndStoreNavCommand _command;
        private readonly ILogger<NavDownloadJob> _logger;

        public NavDownloadJob(DownloadAndStoreNavCommand command, ILogger<NavDownloadJob> logger)
        {
            _command = command;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("========== Job Started at {Time} ==========", DateTime.Now);

            var targetDate = DateHelper.GetTargetNavDate();
            _logger.LogInformation("Target NAV date: {Date}", targetDate.ToString("yyyy-MM-dd"));

            var result = await _command.ExecuteAsync(targetDate);

            if (result.IsSuccess)
            {
                if (result.Data)
                    _logger.LogInformation("✅ Data downloaded and stored successfully");
                else
                    _logger.LogInformation("Data already exists for this date");
            }
            else
            {
                _logger.LogError("❌ Failed: {Error}", result.ErrorMessage);
            }

            _logger.LogInformation("========== Job Completed at {Time} ==========", DateTime.Now);
        }
    }
}