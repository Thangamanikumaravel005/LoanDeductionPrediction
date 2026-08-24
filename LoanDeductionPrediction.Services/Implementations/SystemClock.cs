using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class SystemClock : IClock
    {
        public DateOnly Today =>
            DateOnly.FromDateTime(DateTime.Today);
    }
}