using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Implementations
{
    public class RiskPredictionRepository
        : IRiskPredictionRepository
    {
        private readonly LoanDeductionDbContext _context;

        public RiskPredictionRepository(
            LoanDeductionDbContext context)
        {
            _context = context;
        }

         
        // GET LOAN
         

        public async Task<LoanAccount?> GetLoanAsync(
            int loanId)
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.LoanId == loanId);
        }

         
        // GET PAYMENT BEHAVIOR
         

        public async Task<List<PaymentBehaviorLog>>
            GetBehaviorLogsByLoanIdAsync(
                int loanId)
        {
            return await _context.PaymentBehaviorLogs
                .AsNoTracking()
                .Where(x =>
                    x.LoanId == loanId)
                .OrderBy(x =>
                    x.DueDate)
                .ToListAsync();
        }

         
        // GET PREDICTION BY ID
         

        public async Task<RiskPrediction?>
            GetByIdAsync(int id)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .Include(x => x.Loan)
                .FirstOrDefaultAsync(x =>
                    x.RiskPredictionId == id);
        }

         
        // GET LOAN PREDICTIONS
         

        public async Task<List<RiskPrediction>>
            GetByLoanIdAsync(int loanId)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .Where(x =>
                    x.LoanId == loanId)
                .OrderByDescending(x =>
                    x.PredictionDate)
                .ToListAsync();
        }

         
        // GET BORROWER PREDICTIONS
         

        public async Task<List<RiskPrediction>>
            GetByBorrowerIdAsync(int borrowerId)
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .Where(x =>
                    x.BorrowerId == borrowerId)
                .OrderByDescending(x =>
                    x.PredictionDate)
                .ToListAsync();
        }

         
        // ADD PREDICTION
         

        public async Task<RiskPrediction>
            AddAsync(RiskPrediction prediction)
        {
            _context.RiskPredictions.Add(prediction);

            await _context.SaveChangesAsync();

            return prediction;
        }
    }
}