using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface IRepaymentScheduleRepository
    {
        Task<List<RepaymentSchedule>> GetByLoanIdAsync(
            int loanId);

        Task<RepaymentSchedule?> GetByIdAsync(
            int scheduleId);

        Task<bool> ExistsForLoanAsync(
            int loanId);

        Task AddRangeAsync(
            List<RepaymentSchedule> schedules);

        Task UpdateAsync(
            RepaymentSchedule schedule);

        Task<List<RepaymentSchedule>>
            GetOverdueSchedulesAsync();
    }
}