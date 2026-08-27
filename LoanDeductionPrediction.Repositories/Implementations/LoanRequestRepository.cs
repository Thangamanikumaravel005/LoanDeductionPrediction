using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Implementations
{
    public class LoanRequestRepository
        : ILoanRequestRepository
    {
        private readonly LoanDeductionDbContext _context;

        public LoanRequestRepository(
            LoanDeductionDbContext context)
        {
            _context = context;
        }


        // Get one loan request
        public async Task<LoanRequest?>
            GetByIdAsync(int loanRequestId)
        {
            return await _context.LoanRequests
                .FirstOrDefaultAsync(x =>
                    x.LoanRequestId == loanRequestId);
        }


        // Get requests belonging to one borrower
        public async Task<List<LoanRequest>>
            GetByBorrowerIdAsync(int borrowerId)
        {
            return await _context.LoanRequests
                .AsNoTracking()
                .Where(x =>
                    x.BorrowerId == borrowerId)
                .OrderByDescending(x =>
                    x.RequestedAt)
                .ToListAsync();
        }


        // Get all pending requests
        public async Task<List<LoanRequest>>
            GetPendingAsync()
        {
            return await _context.LoanRequests
                .AsNoTracking()
                .Where(x =>
                    x.Status == "PENDING")
                .OrderBy(x =>
                    x.RequestedAt)
                .ToListAsync();
        }


        // Add new request
        public async Task AddAsync(
            LoanRequest loanRequest)
        {
            await _context.LoanRequests
                .AddAsync(loanRequest);
        }


        // Save changes
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}