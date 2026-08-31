using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> PayEmiAsync(
            int scheduleId);

        Task<List<Payment>> GetByLoanIdAsync(
            int loanId);

        Task<List<Payment>> GetByBorrowerIdAsync(
            int borrowerId);

        Task<Payment?> GetByIdAsync(
            int paymentId);
    }
}