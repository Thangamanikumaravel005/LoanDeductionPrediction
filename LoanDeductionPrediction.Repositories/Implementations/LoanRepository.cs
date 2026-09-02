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


        
        // GET LOAN BY ID
        

        public async Task<LoanAccount?> GetByIdAsync(
            int loanId)
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(l =>
                    l.LoanId == loanId &&
                    l.Status != "DELETED");
        }


        
        // GET ALL LOANS
        

        public async Task<List<LoanAccount>> GetAllAsync()
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .Where(l =>
                    l.Status != "DELETED")
                .OrderByDescending(l => l.LoanId)
                .ToListAsync();
        }


        
        // GET LOANS BY BORROWER
        

        public async Task<List<LoanAccount>>
            GetByBorrowerIdAsync(int borrowerId)
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .Where(l =>
                    l.BorrowerId == borrowerId &&
                    l.Status != "DELETED")
                .OrderByDescending(l => l.LoanId)
                .ToListAsync();
        }


        
        // GET LOANS BY LOAN OFFICER
        

        public async Task<List<LoanAccount>>
            GetByLoanOfficerIdAsync(
                int loanOfficerId)
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .Where(l =>
                    l.LoanOfficerId == loanOfficerId &&
                    l.Status != "DELETED")
                .OrderByDescending(l => l.LoanId)
                .ToListAsync();
        }


        
        // CREATE LOAN
        

        public async Task<LoanAccount> AddAsync(
            LoanAccount loan)
        {
            _context.LoanAccounts.Add(loan);

            await _context.SaveChangesAsync();

            return loan;
        }


        
        // UPDATE LOAN
        

        public async Task UpdateAsync(
            LoanAccount loan)
        {
            _context.LoanAccounts.Update(loan);

            await _context.SaveChangesAsync();
        }
    }
}