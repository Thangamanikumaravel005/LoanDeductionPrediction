namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<object> GetLoanOfficerDashboardAsync(
            int loanOfficerId);

        Task<object> GetAdminDashboardAsync();

        Task<object> GetBorrowerDashboardAsync(
    int borrowerId);
    }
}