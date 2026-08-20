using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<List<LoanAccount>>
            GetLoansByLoanOfficerIdAsync(
                int loanOfficerId);

        Task<List<RiskPrediction>>
            GetRiskPredictionsByLoanOfficerIdAsync(
                int loanOfficerId);

        Task<List<RepaymentSchedule>>
            GetSchedulesByLoanOfficerIdAsync(
                int loanOfficerId);

        Task<List<User>>
            GetAllUsersAsync();

        Task<List<LoanAccount>>
            GetAllLoansAsync();

        Task<List<RepaymentSchedule>>
            GetAllSchedulesAsync();

        Task<List<PaymentBehaviorLog>>
            GetAllBehaviorLogsAsync();

        Task<List<RiskPrediction>>
            GetAllRiskPredictionsAsync();
    }
}