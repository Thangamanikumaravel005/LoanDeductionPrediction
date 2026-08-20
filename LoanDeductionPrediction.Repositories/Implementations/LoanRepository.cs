using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Implementations
{
    public class LoanRepository : ILoanRepository
    {
        private readonly LoanDeductionDbContext _context;

        public LoanRepository(
            LoanDeductionDbContext context)
        {
            _context = context;
        }

        public async Task<LoanAccount?> GetByIdAsync(
            int loanId)
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(l =>
                    l.LoanId == loanId);
        }

        public async Task<List<LoanAccount>> GetAllAsync()
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .OrderByDescending(l => l.LoanId)
                .ToListAsync();
        }

        public async Task<List<LoanAccount>>
            GetByBorrowerIdAsync(int borrowerId)
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .Where(l =>
                    l.BorrowerId == borrowerId)
                .OrderByDescending(l => l.LoanId)
                .ToListAsync();
        }

        public async Task<List<LoanAccount>>
            GetByLoanOfficerIdAsync(int loanOfficerId)
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .Where(l =>
                    l.LoanOfficerId == loanOfficerId)
                .OrderByDescending(l => l.LoanId)
                .ToListAsync();
        }

        public async Task<LoanAccount> AddAsync(
            LoanAccount loan)
        {
            _context.LoanAccounts.Add(loan);

            await _context.SaveChangesAsync();

            return loan;
        }

        public async Task UpdateAsync(
    LoanAccount loan)
{
    _context.LoanAccounts.Update(loan);

    await Task.CompletedTask;
}
    }
}