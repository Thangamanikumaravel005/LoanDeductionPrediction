using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface IRiskPredictionRepository
    {
        Task<LoanAccount?> GetLoanAsync(int loanId);

        Task<List<PaymentBehaviorLog>>
            GetBehaviorLogsByLoanIdAsync(int loanId);

        Task<RiskPrediction?> GetByIdAsync(int id);

        Task<List<RiskPrediction>>
            GetByLoanIdAsync(int loanId);

        Task<List<RiskPrediction>>
            GetByBorrowerIdAsync(int borrowerId);

        Task<RiskPrediction>
            AddAsync(RiskPrediction prediction);
    }
}