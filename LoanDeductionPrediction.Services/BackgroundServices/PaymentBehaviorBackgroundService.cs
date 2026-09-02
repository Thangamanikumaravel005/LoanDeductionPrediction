using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LoanDeductionPrediction.Services.BackgroundServices
{
    public class PaymentBehaviorBackgroundService
        : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PaymentBehaviorBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public PaymentBehaviorBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentBehaviorBackgroundService> logger,
            IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var enabled =
                _configuration.GetValue<bool>(
                    "BackgroundServices:MissedEmiProcessingEnabled",
                    defaultValue: true);

            if (!enabled)
            {
                _logger.LogInformation(
                    "Payment Behavior Background Service is disabled via " +
                    "configuration. Skipping hourly overdue EMI processing.");

                return;
            }

            _logger.LogInformation(
                "Payment Behavior Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var paymentBehaviorService =
                        scope.ServiceProvider
                            .GetRequiredService<IPaymentBehaviorService>();

                    var processedCount =
                        await paymentBehaviorService
                            .ProcessOverdueSchedulesAsync();

                    if (processedCount > 0)
                    {
                        _logger.LogInformation(
                            "Processed {Count} overdue repayment schedules.",
                            processedCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while processing overdue repayment schedules.");
                }

                // Check once every hour.
                await Task.Delay(
                    TimeSpan.FromHours(1),
                    stoppingToken);
            }

            _logger.LogInformation(
                "Payment Behavior Background Service stopped.");
        }
    }
}