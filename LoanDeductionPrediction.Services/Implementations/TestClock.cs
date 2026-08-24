using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class TestClock : IClock
    {
        public DateOnly Today { get; set; }
    }
}