using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface ILoanRequestService
    {
        Task<LoanRequest> CreateRequestAsync(
            int borrowerId,
            CreateLoanRequestDto request);

        Task<List<LoanRequest>> GetMyRequestsAsync(
            int borrowerId);

        Task<List<LoanRequest>> GetPendingRequestsAsync();

        Task<LoanRequest?> GetByIdAsync(
            int loanRequestId);

       Task<bool> ApproveRequestAsync(
    int loanRequestId,
    int loanOfficerId,
    decimal interestRate,
    int tenureMonths);

        Task<bool> RejectRequestAsync(
            int loanRequestId,
            int loanOfficerId,
            string? remarks);
    }
}