using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Implementations
{
    public class BorrowerLoanApplicationRepository : IBorrowerLoanApplicationRepository
    {
        private readonly LoanDeductionDbContext _context;

        public BorrowerLoanApplicationRepository(LoanDeductionDbContext context)
        {
            _context = context;
        }

        public async Task<BorrowerLoanApplication> AddAsync(BorrowerLoanApplication application)
        {
            _context.BorrowerLoanApplications.Add(application);
            await _context.SaveChangesAsync();
            return application;
        }

        public async Task<BorrowerLoanApplication?> GetByIdAsync(int applicationId)
        {
            return await _context.BorrowerLoanApplications
                .Include(a => a.ReviewedByLoanOfficer)
                .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
        }

       public async Task<List<BorrowerLoanApplication>> GetPendingAsync()
{
    return await _context.BorrowerLoanApplications
        .Where(a => a.Status == "PENDING")
        .OrderByDescending(a => a.CreatedAt)
        .ToListAsync();
}

        public async Task<List<BorrowerLoanApplication>>
    GetByEmailAsync(string email)
{
    var normalizedEmail = email.Trim().ToLower();

    return await _context.BorrowerLoanApplications
        .Include(a => a.ReviewedByLoanOfficer)
        .Where(a => a.Email.ToLower() == normalizedEmail)
        .OrderByDescending(a => a.CreatedAt)
        .ToListAsync();
}


// NEW METHOD
public async Task<List<BorrowerLoanApplication>>
    GetByBorrowerIdAsync(int borrowerId)
{
    return await _context.BorrowerLoanApplications
        .Include(a => a.ReviewedByLoanOfficer)
        .Where(a => a.BorrowerId == borrowerId)
        .OrderByDescending(a => a.CreatedAt)
        .ToListAsync();
}


        public async Task UpdateAsync(BorrowerLoanApplication application)
        {
            _context.BorrowerLoanApplications.Update(application);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
