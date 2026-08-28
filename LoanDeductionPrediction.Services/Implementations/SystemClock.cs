using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class SystemClock : IClock
    {
        private readonly IConfiguration _configuration;

        public SystemClock(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DateOnly Today
        {
            get
            {
                var useTestDate =
                    _configuration.GetValue<bool>(
                        "TestSettings:UseTestDate");

                if (useTestDate)
                {
                    var testDate =
                        _configuration.GetValue<DateTime>(
                            "TestSettings:TestDate");

                    return DateOnly.FromDateTime(testDate);
                }

                return DateOnly.FromDateTime(
                    DateTime.Today);
            }
        }
    }
}