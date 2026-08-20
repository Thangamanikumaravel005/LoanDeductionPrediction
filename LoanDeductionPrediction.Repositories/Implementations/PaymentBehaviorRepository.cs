using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Implementations
{
    public class PaymentBehaviorRepository
        : IPaymentBehaviorRepository
    {
        private readonly LoanDeductionDbContext _context;

        public PaymentBehaviorRepository(
            LoanDeductionDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentBehaviorLog>>
            GetByBorrowerIdAsync(int borrowerId)
        {
            return await _context.PaymentBehaviorLogs
                .AsNoTracking()
                .Include(x => x.Borrower)
                .Where(x => x.BorrowerId == borrowerId)
                .OrderByDescending(x => x.DueDate)
                .ToListAsync();
        }

        public async Task<List<PaymentBehaviorLog>>
            GetByLoanIdAsync(int loanId)
        {
            return await _context.PaymentBehaviorLogs
                .AsNoTracking()
                .Where(x => x.LoanId == loanId)
                .OrderBy(x => x.DueDate)
                .ToListAsync();
        }

        public async Task<PaymentBehaviorLog?>
            GetByIdAsync(int id)
        {
            return await _context.PaymentBehaviorLogs
                .Include(x => x.Loan)
                .FirstOrDefaultAsync(x =>
                    x.BehaviorLogId == id);
        }

        public async Task<PaymentBehaviorLog?>
            GetByScheduleIdAsync(int scheduleId)
        {
            return await _context.PaymentBehaviorLogs
                .FirstOrDefaultAsync(x =>
                    x.ScheduleId == scheduleId);
        }

         
        // ADD + SAVE
        // Used by RecordBehaviorAsync()
         

        public async Task<PaymentBehaviorLog>
            AddAsync(PaymentBehaviorLog log)
        {
            _context.PaymentBehaviorLogs.Add(log);

            await _context.SaveChangesAsync();

            return log;
        }

         
        // ADD WITHOUT SAVE
        // Used inside payment transaction
         

        public async Task<PaymentBehaviorLog>
            AddWithoutSaveAsync(PaymentBehaviorLog log)
        {
            _context.PaymentBehaviorLogs.Add(log);

            await Task.CompletedTask;

            return log;
        }

        public async Task<bool>
            BorrowerExistsAsync(int borrowerId)
        {
            return await _context.Users
                .AsNoTracking()
                .AnyAsync(u =>
                    u.UserId == borrowerId &&
                    u.Role == "Borrower");
        }
    }
}