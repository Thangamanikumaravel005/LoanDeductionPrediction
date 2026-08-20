using LoanDeductionPrediction.Services.Interfaces;
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

        public PaymentBehaviorBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<PaymentBehaviorBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
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