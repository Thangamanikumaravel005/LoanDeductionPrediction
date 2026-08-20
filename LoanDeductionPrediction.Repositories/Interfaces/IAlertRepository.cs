using LoanDeductionPrediction.Models.DTOs;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface IAlertRepository
    {
        Task<List<AlertDto>> GetAlertsAsync(
            string role,
            int userId);

        Task<List<AlertDto>> GetLoanAlertsAsync(
            int loanId);
    }
}