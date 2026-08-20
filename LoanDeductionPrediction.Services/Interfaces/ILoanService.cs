using LoanDeductionPrediction.Models.DTOs;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface ILoanService
    {
        Task<LoanDto?> GetByIdAsync(int loanId);

        Task<List<LoanDto>> GetAllAsync();

        Task<List<LoanDto>> GetByBorrowerIdAsync(
            int borrowerId);

        Task<List<LoanDto>> GetByLoanOfficerIdAsync(
            int loanOfficerId);

        Task<LoanDto> CreateAsync(
            CreateLoanRequest request);

        Task<bool> UpdateStatusAsync(
            int loanId,
            string status);
    }
}