using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface ILoanRepository
    {

        Task<bool> DeleteAsync(int loanId);
        Task<LoanAccount?> GetByIdAsync(int loanId);

        Task<List<LoanAccount>> GetAllAsync();

        Task<List<LoanAccount>> GetByBorrowerIdAsync(
            int borrowerId);

        Task<List<LoanAccount>> GetByLoanOfficerIdAsync(
            int loanOfficerId);

        Task<LoanAccount> AddAsync(LoanAccount loan);

        Task UpdateAsync(LoanAccount loan);
    }
}