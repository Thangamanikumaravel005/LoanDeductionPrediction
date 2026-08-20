using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IRiskPredictionService
    {
        Task<RiskPrediction> GeneratePredictionAsync(
            int loanId);

        Task<RiskPrediction?>
            GetByIdAsync(int id);

        Task<List<RiskPrediction>>
            GetByLoanIdAsync(int loanId);

        Task<List<RiskPrediction>>
            GetByBorrowerIdAsync(int borrowerId);
    }
}