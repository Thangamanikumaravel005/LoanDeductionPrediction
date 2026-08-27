using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface ILoanRequestRepository
    {
        Task<LoanRequest?> GetByIdAsync(
            int loanRequestId);

        Task<List<LoanRequest>> GetByBorrowerIdAsync(
            int borrowerId);

        Task<List<LoanRequest>> GetPendingAsync();

        Task AddAsync(
            LoanRequest loanRequest);

        Task SaveChangesAsync();
    }
}