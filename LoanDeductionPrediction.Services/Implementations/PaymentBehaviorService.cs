using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class PaymentBehaviorService
        : IPaymentBehaviorService
    {
        private readonly IPaymentBehaviorRepository
            _behaviorRepository;

        private readonly IRepaymentScheduleRepository
            _scheduleRepository;

        private readonly ILoanRepository
            _loanRepository;

        public PaymentBehaviorService(
            IPaymentBehaviorRepository behaviorRepository,
            IRepaymentScheduleRepository scheduleRepository,
            ILoanRepository loanRepository)
        {
            _behaviorRepository =
                behaviorRepository;

            _scheduleRepository =
                scheduleRepository;

            _loanRepository =
                loanRepository;
        }

         
        // GET BY BORROWER
         

        public async Task<List<PaymentBehaviorLog>>
            GetByBorrowerIdAsync(int borrowerId)
        {
            if (borrowerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid borrower ID.");
            }

            var borrowerExists =
                await _behaviorRepository
                    .BorrowerExistsAsync(
                        borrowerId);

            if (!borrowerExists)
            {
                throw new ArgumentException(
                    "Borrower not found.");
            }

            return await _behaviorRepository
                .GetByBorrowerIdAsync(
                    borrowerId);
        }

         
        // GET BY LOAN
         

        public async Task<List<PaymentBehaviorLog>>
            GetByLoanIdAsync(int loanId)
        {
            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan ID.");
            }

            var loan =
                await _loanRepository
                    .GetByIdAsync(loanId);

            if (loan == null)
            {
                throw new ArgumentException(
                    "Loan not found.");
            }

            return await _behaviorRepository
                .GetByLoanIdAsync(
                    loanId);
        }

         
        // GET BY ID
         

        public async Task<PaymentBehaviorLog?>
            GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _behaviorRepository
                .GetByIdAsync(id);
        }

         
        // RECORD PAYMENT BEHAVIOR
         

        public async Task<PaymentBehaviorLog>
            RecordBehaviorAsync(
                int scheduleId)
        {
             
            // Validate schedule ID
             

            if (scheduleId <= 0)
            {
                throw new ArgumentException(
                    "Invalid repayment schedule ID.");
            }

             
            // Get repayment schedule
             

            var schedule =
                await _scheduleRepository
                    .GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                throw new ArgumentException(
                    "Repayment schedule not found.");
            }

             
            // Get related loan
             

            var loan =
                await _loanRepository
                    .GetByIdAsync(
                        schedule.LoanId);

            if (loan == null)
            {
                throw new ArgumentException(
                    "Loan not found.");
            }

             
            // Prevent duplicate behavior record
             

            var existingLog =
                await _behaviorRepository
                    .GetByScheduleIdAsync(
                        scheduleId);

            if (existingLog != null)
            {
                throw new InvalidOperationException(
                    "Payment behavior has already been recorded for this installment.");
            }

             
            // Determine payment behavior
             

            var paymentDate =
                schedule.PaidDate;

            int daysLate = 0;

            string paymentStatus;

             
            // PAID
             

            if (string.Equals(
                    schedule.Status,
                    "PAID",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (paymentDate.HasValue &&
                    paymentDate.Value >
                    schedule.DueDate)
                {
                    daysLate =
                        paymentDate.Value.DayNumber -
                        schedule.DueDate.DayNumber;

                    paymentStatus =
                        "LATE";
                }
                else
                {
                    paymentStatus =
                        "ON_TIME";
                }
            }

             
            // PARTIAL
             

            else if (string.Equals(
                        schedule.Status,
                        "PARTIAL",
                        StringComparison.OrdinalIgnoreCase))
            {
                paymentStatus =
                    "PARTIAL";

                if (paymentDate.HasValue &&
                    paymentDate.Value >
                    schedule.DueDate)
                {
                    daysLate =
                        paymentDate.Value.DayNumber -
                        schedule.DueDate.DayNumber;
                }
            }

             
            // PENDING / MISSED
             

            else
            {
                var today =
                    DateOnly.FromDateTime(
                        DateTime.Today);

                if (today >
                    schedule.DueDate)
                {
                    daysLate =
                        today.DayNumber -
                        schedule.DueDate.DayNumber;

                    paymentStatus =
                        "MISSED";
                }
                else
                {
                    paymentStatus =
                        "PENDING";
                }
            }

             
            // Create behavior log
             

            var log =
                new PaymentBehaviorLog
                {
                    BorrowerId =
                        loan.BorrowerId,

                    LoanId =
                        loan.LoanId,

                    ScheduleId =
                        schedule.ScheduleId,

                    DueDate =
                        schedule.DueDate,

                    PaymentDate =
                        paymentDate,

                    DaysLate =
                        daysLate,

                    PaymentStatus =
                        paymentStatus,

                    RecordedAt =
                        DateTime.UtcNow
                };

             
            // Add behavior log
             

            return await _behaviorRepository
                .AddAsync(log);
        }

         
        // CREATE BEHAVIOR LOG FOR ALREADY UPDATED PAYMENT
         

        public async Task<PaymentBehaviorLog>
            CreateBehaviorLogAsync(
                RepaymentSchedule schedule,
                LoanAccount loan)
        {
             
            // Validate schedule
             

            if (schedule == null)
            {
                throw new ArgumentNullException(
                    nameof(schedule));
            }

             
            // Validate loan
             

            if (loan == null)
            {
                throw new ArgumentNullException(
                    nameof(loan));
            }

             
            // Check duplicate behavior record
             

            var existingLog =
                await _behaviorRepository
                    .GetByScheduleIdAsync(
                        schedule.ScheduleId);

            if (existingLog != null)
            {
                return existingLog;
            }

             
            // Get payment date
             

            var paymentDate =
                schedule.PaidDate;

            int daysLate = 0;

            string paymentStatus;

             
            // PAID
             

            if (string.Equals(
                    schedule.Status,
                    "PAID",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (paymentDate.HasValue &&
                    paymentDate.Value >
                    schedule.DueDate)
                {
                    daysLate =
                        paymentDate.Value.DayNumber -
                        schedule.DueDate.DayNumber;

                    paymentStatus =
                        "LATE";
                }
                else
                {
                    paymentStatus =
                        "ON_TIME";
                }
            }

             
            // PARTIAL
             

            else if (string.Equals(
                        schedule.Status,
                        "PARTIAL",
                        StringComparison.OrdinalIgnoreCase))
            {
                paymentStatus =
                    "PARTIAL";

                if (paymentDate.HasValue &&
                    paymentDate.Value >
                    schedule.DueDate)
                {
                    daysLate =
                        paymentDate.Value.DayNumber -
                        schedule.DueDate.DayNumber;
                }
            }

             
            // PENDING / MISSED
             

            else
            {
                var today =
                    DateOnly.FromDateTime(
                        DateTime.Today);

                if (today >
                    schedule.DueDate)
                {
                    daysLate =
                        today.DayNumber -
                        schedule.DueDate.DayNumber;

                    paymentStatus =
                        "MISSED";
                }
                else
                {
                    paymentStatus =
                        "PENDING";
                }
            }

             
            // CREATE BEHAVIOR LOG
             

            var log =
                new PaymentBehaviorLog
                {
                    BorrowerId =
                        loan.BorrowerId,

                    LoanId =
                        loan.LoanId,

                    ScheduleId =
                        schedule.ScheduleId,

                    DueDate =
                        schedule.DueDate,

                    PaymentDate =
                        paymentDate,

                    DaysLate =
                        daysLate,

                    PaymentStatus =
                        paymentStatus,

                    RecordedAt =
                        DateTime.UtcNow
                };

             
            // ADD LOG TO CONTEXT
             

            return await _behaviorRepository
                .AddAsync(log);
        }

         
        // PROCESS OVERDUE REPAYMENT SCHEDULES
         

        public async Task<int>
            ProcessOverdueSchedulesAsync()
        {
            var overdueSchedules =
                await _scheduleRepository
                    .GetOverdueSchedulesAsync();

            var processedCount = 0;

            foreach (var schedule in overdueSchedules)
            {
                 
                // Check whether behavior was already recorded
                 

                var existingLog =
                    await _behaviorRepository
                        .GetByScheduleIdAsync(
                            schedule.ScheduleId);

                if (existingLog != null)
                {
                    continue;
                }

                 
                // Record behavior
                 

                await RecordBehaviorAsync(
                    schedule.ScheduleId);

                processedCount++;
            }

            return processedCount;
        }
    }
}