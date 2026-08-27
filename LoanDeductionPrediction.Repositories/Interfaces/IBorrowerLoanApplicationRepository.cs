using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface IBorrowerLoanApplicationRepository
    {
        Task<BorrowerLoanApplication> AddAsync(BorrowerLoanApplication application);

        Task<BorrowerLoanApplication?> GetByIdAsync(int applicationId);

        Task<List<BorrowerLoanApplication>> GetPendingAsync();

        Task<List<BorrowerLoanApplication>> GetByEmailAsync(string email);

        Task UpdateAsync(BorrowerLoanApplication application);

        Task SaveChangesAsync();
    }
}
