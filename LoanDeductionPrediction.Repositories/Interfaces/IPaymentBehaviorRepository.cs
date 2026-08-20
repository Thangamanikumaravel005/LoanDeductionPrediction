using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface IPaymentBehaviorRepository
    {
        Task<List<PaymentBehaviorLog>>
            GetByBorrowerIdAsync(
                int borrowerId);

        Task<List<PaymentBehaviorLog>>
            GetByLoanIdAsync(
                int loanId);

        Task<PaymentBehaviorLog?>
            GetByIdAsync(
                int id);

        Task<PaymentBehaviorLog?>
            GetByScheduleIdAsync(
                int scheduleId);

        // Add and save immediately
        Task<PaymentBehaviorLog>
            AddAsync(
                PaymentBehaviorLog log);

        // Add only - UnitOfWork will save
        Task<PaymentBehaviorLog>
            AddWithoutSaveAsync(
                PaymentBehaviorLog log);

        Task<bool>
            BorrowerExistsAsync(
                int borrowerId);
    }
}