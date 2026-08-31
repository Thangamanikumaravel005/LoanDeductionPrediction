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


        // =========================================================
        // GET LOAN BY ID
        // =========================================================

        public async Task<LoanAccount?> GetByIdAsync(
            int loanId)
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(l =>
                    l.LoanId == loanId);
        }


        // =========================================================
        // GET ALL LOANS
        // =========================================================

        public async Task<List<LoanAccount>> GetAllAsync()
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .OrderByDescending(l => l.LoanId)
                .ToListAsync();
        }


        // =========================================================
        // GET LOANS BY BORROWER
        // =========================================================

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


        // =========================================================
        // GET LOANS BY LOAN OFFICER
        // =========================================================

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


        // =========================================================
        // CREATE LOAN
        // =========================================================

        public async Task<LoanAccount> AddAsync(
            LoanAccount loan)
        {
            _context.LoanAccounts.Add(loan);

            await _context.SaveChangesAsync();

            return loan;
        }


        // =========================================================
        // UPDATE LOAN
        // =========================================================

        public async Task UpdateAsync(
            LoanAccount loan)
        {
            _context.LoanAccounts.Update(loan);

            await Task.CompletedTask;
        }


        // =========================================================
        // DELETE LOAN + STORE HISTORY
        // =========================================================

        public async Task<bool> DeleteAsync(
            int loanId)
        {
            // Find the loan
            var loan = await _context.LoanAccounts
                .FirstOrDefaultAsync(l =>
                    l.LoanId == loanId);

            // Loan does not exist
            if (loan == null)
            {
                return false;
            }


            // =====================================================
            // CREATE LOAN HISTORY BEFORE DELETE
            // =====================================================

            var history = new LoanHistory
            {
                OriginalLoanId =
                    loan.LoanId,

                BorrowerId =
                    loan.BorrowerId,

                LoanOfficerId =
                    loan.LoanOfficerId,

                PrincipalAmount =
                    loan.PrincipalAmount,

                InterestRate =
                    loan.InterestRate,

                TenureMonths =
                    loan.TenureMonths,

                StartDate =
                    loan.StartDate,

                EndDate =
                    loan.EndDate,

                OutstandingAmount =
                    loan.OutstandingAmount,

                Status =
                    loan.Status,

                CreatedAt =
                    loan.CreatedAt,

                DeletedAt =
                    DateTime.UtcNow
            };


            // =====================================================
            // ADD HISTORY
            // =====================================================

            await _context.LoanHistories
                .AddAsync(history);


            // =====================================================
            // DELETE ORIGINAL LOAN
            // =====================================================

            _context.LoanAccounts
                .Remove(loan);


            // =====================================================
            // SAVE BOTH OPERATIONS
            // =====================================================

            await _context.SaveChangesAsync();


            return true;
        }
    }
}