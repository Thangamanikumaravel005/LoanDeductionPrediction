using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly LoanDeductionDbContext _context;

        public PaymentRepository(
            LoanDeductionDbContext context)
        {
            _context = context;
        }

       
        // ADD PAYMENT
       

        public async Task<Payment> AddAsync(
            Payment payment)
        {
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return payment;
        }

       
        // GET PAYMENTS BY LOAN
       

        public async Task<List<Payment>> GetByLoanIdAsync(
            int loanId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.LoanId == loanId)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .ToListAsync();
        }

       
        // GET PAYMENTS BY BORROWER
       

        public async Task<List<Payment>> GetByBorrowerIdAsync(
            int borrowerId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.BorrowerId == borrowerId)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentId)
                .ToListAsync();
        }

       
        // GET PAYMENT BY ID
       

        public async Task<Payment?> GetByIdAsync(
            int paymentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.PaymentId == paymentId);
        }
    }
}