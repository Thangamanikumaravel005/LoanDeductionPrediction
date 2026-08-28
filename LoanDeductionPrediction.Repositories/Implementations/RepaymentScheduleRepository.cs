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

        public async Task<RepaymentSchedule?>
            GetByIdAsync(int scheduleId)
        {
            return await _context
                .RepaymentSchedules
                .FirstOrDefaultAsync(
                    x => x.ScheduleId == scheduleId);
        }

        public async Task<bool>
            ExistsForLoanAsync(int loanId)
        {
            return await _context
                .RepaymentSchedules
                .AnyAsync(
                    x => x.LoanId == loanId);
        }

        public async Task AddRangeAsync(
            List<RepaymentSchedule> schedules)
        {
            await _context
                .RepaymentSchedules
                .AddRangeAsync(schedules);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            RepaymentSchedule schedule)
        {
            _context
                .RepaymentSchedules
                .Update(schedule);

            await _context.SaveChangesAsync();
        }

        public async Task<List<RepaymentSchedule>>
            GetOverdueSchedulesAsync(
                DateOnly today)
        {
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