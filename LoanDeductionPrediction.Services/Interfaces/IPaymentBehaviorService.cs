using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IPaymentBehaviorService
    {
         
        // GET BEHAVIOR BY BORROWER
         

        Task<List<PaymentBehaviorLog>>
            GetByBorrowerIdAsync(
                int borrowerId);

         
        // GET BEHAVIOR BY LOAN
         

        Task<List<PaymentBehaviorLog>>
            GetByLoanIdAsync(
                int loanId);

         
        // GET BEHAVIOR BY ID
         

        Task<PaymentBehaviorLog?>
            GetByIdAsync(
                int id);

         
        // RECORD PAYMENT BEHAVIOR
         

        Task<PaymentBehaviorLog>
            RecordBehaviorAsync(
                int scheduleId);

         
        // CREATE BEHAVIOR LOG
        // FOR AN ALREADY UPDATED PAYMENT
         

        Task<PaymentBehaviorLog>
            CreateBehaviorLogAsync(
                RepaymentSchedule schedule,
                LoanAccount loan);

         
        // PROCESS OVERDUE SCHEDULES
         

        Task<int>
            ProcessOverdueSchedulesAsync();
    }
}