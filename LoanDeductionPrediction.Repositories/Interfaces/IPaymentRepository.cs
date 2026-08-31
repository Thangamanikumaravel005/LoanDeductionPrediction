using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> AddAsync(
            Payment payment);

        Task<List<Payment>> GetByLoanIdAsync(
            int loanId);

        Task<List<Payment>> GetByBorrowerIdAsync(
            int borrowerId);

        Task<Payment?> GetByIdAsync(
            int paymentId);
    }
}