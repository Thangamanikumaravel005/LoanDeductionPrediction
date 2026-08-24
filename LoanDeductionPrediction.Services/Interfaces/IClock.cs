namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IClock
    {
        DateOnly Today { get; }
    }
}