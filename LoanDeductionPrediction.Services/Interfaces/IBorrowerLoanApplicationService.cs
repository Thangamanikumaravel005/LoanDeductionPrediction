using LoanDeductionPrediction.Models.DTOs;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IBorrowerLoanApplicationService
    {
        Task<BorrowerLoanApplicationDto> SubmitApplicationAsync(
            CreateBorrowerLoanApplicationRequest request);

        Task<List<BorrowerLoanApplicationDto>> GetPendingApplicationsAsync();

        Task<BorrowerLoanApplicationDto?> GetByIdAsync(
            int applicationId);

        Task<List<BorrowerLoanApplicationDto>> GetMyApplicationsAsync(
            int userId);

        Task<AcceptBorrowerLoanApplicationResponse> AcceptApplicationAsync(
            int applicationId,
            int loanOfficerId,
            ApproveBorrowerLoanApplicationRequest request);

        Task<BorrowerLoanApplicationDto> RejectApplicationAsync(
            int applicationId,
            int loanOfficerId,
            RejectBorrowerLoanApplicationRequest request);
    }
}
