using LoanDeductionPrediction.Models.DTOs;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IAlertService
    {
        Task<List<AlertDto>> GetAlertsAsync(
            string role,
            int userId);

        Task<List<AlertDto>> GetLoanAlertsAsync(
            int loanId);
    }
}