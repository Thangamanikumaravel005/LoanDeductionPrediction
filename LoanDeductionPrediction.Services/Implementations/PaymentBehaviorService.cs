using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class PaymentBehaviorService : IPaymentBehaviorService
    {
        private readonly IPaymentBehaviorRepository _behaviorRepository;
        private readonly IRepaymentScheduleRepository _scheduleRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IClock _clock;
        private readonly ILogger<PaymentBehaviorService> _logger;

        public PaymentBehaviorService(
            IPaymentBehaviorRepository behaviorRepository,
            IRepaymentScheduleRepository scheduleRepository,
            ILoanRepository loanRepository,
            IClock clock,
            ILogger<PaymentBehaviorService> logger)
        {
            _behaviorRepository = behaviorRepository;
            _scheduleRepository = scheduleRepository;
            _loanRepository = loanRepository;
            _clock = clock;
            _logger = logger;
        }

        
        // GET PAYMENT BEHAVIOR BY BORROWER
        

        public async Task<List<PaymentBehaviorLog>>
            GetByBorrowerIdAsync(int borrowerId)
        {
            return await _behaviorRepository
                .GetByBorrowerIdAsync(borrowerId);
        }

        
        // GET PAYMENT BEHAVIOR BY LOAN
        

        public async Task<List<PaymentBehaviorLog>>
            GetByLoanIdAsync(int loanId)
        {
            return await _behaviorRepository
                .GetByLoanIdAsync(loanId);
        }

        
        // GET PAYMENT BEHAVIOR BY ID
        

        public async Task<PaymentBehaviorLog?>
            GetByIdAsync(int id)
        {
            return await _behaviorRepository
                .GetByIdAsync(id);
        }

        
        // RECORD PAYMENT BEHAVIOR
        

        public async Task<PaymentBehaviorLog>
            RecordBehaviorAsync(int scheduleId)
        {
            var schedule =
                await _scheduleRepository
                    .GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                throw new ArgumentException(
                    $"Repayment schedule {scheduleId} was not found.");
            }

            // Check whether behavior already exists
            var existingLog =
                await _behaviorRepository
                    .GetByScheduleIdAsync(scheduleId);

            if (existingLog != null)
            {
                return existingLog;
            }

            // Get loan
            var loan =
                await _loanRepository
                    .GetByIdAsync(schedule.LoanId);

            if (loan == null)
            {
                throw new ArgumentException(
                    $"Loan {schedule.LoanId} was not found.");
            }

            return await CreateBehaviorLogAsync(
                schedule,
                loan);
        }

        
        // CREATE PAYMENT BEHAVIOR LOG
        

        public async Task<PaymentBehaviorLog>
            CreateBehaviorLogAsync(
                RepaymentSchedule schedule,
                LoanAccount loan)
        {
            var today = _clock.Today;

            string paymentStatus;

            // Fully paid
            if (schedule.PaidAmount >= schedule.Emiamount)
            {
                paymentStatus = "PAID";
            }
            // Due date has passed and payment is incomplete
            else if (schedule.DueDate < today)
            {
                paymentStatus = "MISSED";
            }
            // Due date has not arrived
            else
            {
                paymentStatus = "PENDING";
            }

            var daysLate = 0;

            if (paymentStatus == "MISSED")
            {
                daysLate =
                    today.DayNumber -
                    schedule.DueDate.DayNumber;
            }

            var log = new PaymentBehaviorLog
{
    LoanId = schedule.LoanId,
    ScheduleId = schedule.ScheduleId,
    BorrowerId = loan.BorrowerId,
    DueDate = schedule.DueDate,
    PaymentStatus = paymentStatus,
    DaysLate = daysLate
};

            _logger.LogInformation(
                "Creating payment behavior log. " +
                "ScheduleId={ScheduleId}, " +
                "LoanId={LoanId}, " +
                "Status={Status}, " +
                "DueDate={DueDate}, " +
                "Today={Today}, " +
                "DaysLate={DaysLate}",
                schedule.ScheduleId,
                schedule.LoanId,
                paymentStatus,
                schedule.DueDate,
                today,
                daysLate);

            return await _behaviorRepository
                .AddAsync(log);
        }

        
        // PROCESS OVERDUE / MISSED EMIs
        //
        // Swagger:
        //
        // POST /api/PaymentBehavior/process-overdue
        //
        

        public async Task<int>
            ProcessOverdueSchedulesAsync()
        {
            var today = _clock.Today;

            _logger.LogInformation(
                "============================================");

            _logger.LogInformation(
                "Starting overdue EMI processing.");

            _logger.LogInformation(
                "System/Test Date: {Today}",
                today);

          
            // Get overdue schedules
          

            var overdueSchedules =
                await _scheduleRepository
                    .GetOverdueSchedulesAsync(today);

            _logger.LogInformation(
                "Found {Count} overdue repayment schedules.",
                overdueSchedules.Count);

            var processedCount = 0;

          
            // Process each overdue schedule
          

            foreach (var schedule in overdueSchedules)
            {
                _logger.LogInformation(
                    "Processing ScheduleId={ScheduleId}, " +
                    "LoanId={LoanId}, " +
                    "DueDate={DueDate}, " +
                    "CurrentStatus={Status}, " +
                    "PaidAmount={PaidAmount}, " +
                    "EMI={EmiAmount}",
                    schedule.ScheduleId,
                    schedule.LoanId,
                    schedule.DueDate,
                    schedule.Status,
                    schedule.PaidAmount,
                    schedule.Emiamount);

               
                // Safety check:
                // fully paid EMI should never become MISSED
               

                if (schedule.PaidAmount >= schedule.Emiamount)
                {
                    _logger.LogInformation(
                        "ScheduleId={ScheduleId} is fully paid. Skipping.",
                        schedule.ScheduleId);

                    continue;
                }

               
                // Do not re-count schedules that are already MISSED
                // (this makes the method safe to run multiple times).
               

                var wasAlreadyMissed =
                    schedule.Status == "MISSED";

               
                // Get loan
               

                var loan =
                    await _loanRepository
                        .GetByIdAsync(schedule.LoanId);

                if (loan == null)
                {
                    _logger.LogWarning(
                        "LoanId={LoanId} was not found. Skipping ScheduleId={ScheduleId}.",
                        schedule.LoanId,
                        schedule.ScheduleId);

                    continue;
                }

               

                schedule.Status = "MISSED";

               
                // Check whether behavior log already exists
               

                var existingLog =
                    await _behaviorRepository
                        .GetByScheduleIdAsync(
                            schedule.ScheduleId);

                if (existingLog == null)
                {
                    // Create MISSED behavior log.
                    //
                    // CreateBehaviorLogAsync()
                    // also saves through AddAsync(), which also
                    // persists the tracked schedule.Status change.
                    await CreateBehaviorLogAsync(
                        schedule,
                        loan);

                    _logger.LogInformation(
                        "ScheduleId={ScheduleId} changed to MISSED and behavior log created.",
                        schedule.ScheduleId);
                }
                else
                {
                    // Behavior already exists, so no duplicate log is
                    // created. Only persist the changed schedule status.
                    await _scheduleRepository
                        .UpdateAsync(schedule);

                    _logger.LogInformation(
                        "Existing behavior log found for ScheduleId={ScheduleId}. Skipping log creation; schedule persisted as MISSED.",
                        schedule.ScheduleId);
                }

                // Only count schedules that were not already MISSED
                // before this run.
                if (!wasAlreadyMissed)
                {
                    processedCount++;
                }
                else
                {
                    _logger.LogInformation(
                        "ScheduleId={ScheduleId} was already MISSED. Not counted in processedCount.",
                        schedule.ScheduleId);
                }
            }

            _logger.LogInformation(
                "Finished overdue EMI processing.");

            _logger.LogInformation(
                "Processed Count: {Count}",
                processedCount);

            _logger.LogInformation(
                "============================================");

            return processedCount;
        }
    }
}