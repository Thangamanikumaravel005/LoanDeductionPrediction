using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Implementations
{
    public class RepaymentScheduleRepository
        : IRepaymentScheduleRepository
    {
        private readonly LoanDeductionDbContext _context;

        public RepaymentScheduleRepository(
            LoanDeductionDbContext context)
        {
            _context = context;
        }

         
        // GET ALL SCHEDULES FOR A LOAN
         

        public async Task<List<RepaymentSchedule>>
            GetByLoanIdAsync(int loanId)
        {
            return await _context
                .RepaymentSchedules
                .AsNoTracking()
                .Where(x => x.LoanId == loanId)
                .OrderBy(x => x.InstallmentNumber)
                .ToListAsync();
        }

         
        // GET SCHEDULE BY ID
         

        public async Task<RepaymentSchedule?>
            GetByIdAsync(int scheduleId)
        {
            return await _context
                .RepaymentSchedules
                .FirstOrDefaultAsync(
                    x => x.ScheduleId == scheduleId);
        }

         
        // CHECK WHETHER SCHEDULE EXISTS
         

        public async Task<bool>
            ExistsForLoanAsync(int loanId)
        {
            return await _context
                .RepaymentSchedules
                .AnyAsync(
                    x => x.LoanId == loanId);
        }

         
        // ADD REPAYMENT SCHEDULES
         

        public async Task AddRangeAsync(
            List<RepaymentSchedule> schedules)
        {
            await _context
                .RepaymentSchedules
                .AddRangeAsync(schedules);

            await _context.SaveChangesAsync();
        }

         
        // UPDATE REPAYMENT SCHEDULE
         
        // IMPORTANT:
        // Do NOT call SaveChangesAsync() here.
        // The UnitOfWork handles the final save.
         

        public async Task UpdateAsync(
            RepaymentSchedule schedule)
        {
            _context
                .RepaymentSchedules
                .Update(schedule);

            await Task.CompletedTask;
        }

         
        // GET OVERDUE REPAYMENT SCHEDULES
         

        public async Task<List<RepaymentSchedule>>
            GetOverdueSchedulesAsync()
        {
            var today =
                DateOnly.FromDateTime(
                    DateTime.Today);

            return await _context
                .RepaymentSchedules
                .Where(x =>
                    x.DueDate < today &&
                    x.PaidAmount < x.Emiamount &&
                    x.Status != "PAID")
                .OrderBy(x => x.DueDate)
                .ToListAsync();
        }
    }
}