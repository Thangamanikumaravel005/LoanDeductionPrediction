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

        // =========================================================
        // GET PAYMENT BEHAVIOR BY BORROWER
        // =========================================================

        public async Task<List<PaymentBehaviorLog>>
            GetByBorrowerIdAsync(int borrowerId)
        {
            return await _behaviorRepository
                .GetByBorrowerIdAsync(borrowerId);
        }

        // =========================================================
        // GET PAYMENT BEHAVIOR BY LOAN
        // =========================================================

        public async Task<List<PaymentBehaviorLog>>
            GetByLoanIdAsync(int loanId)
        {
            return await _behaviorRepository
                .GetByLoanIdAsync(loanId);
        }

        // =========================================================
        // GET PAYMENT BEHAVIOR BY ID
        // =========================================================

        public async Task<PaymentBehaviorLog?>
            GetByIdAsync(int id)
        {
            return await _behaviorRepository
                .GetByIdAsync(id);
        }

        // =========================================================
        // RECORD PAYMENT BEHAVIOR
        // =========================================================

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

        // =========================================================
        // PROCESS ONE SPECIFIC MISSED EMI
        //
        // POST /api/PaymentBehavior/process-missed/{scheduleId}
        //
        // Example:
        // POST /api/PaymentBehavior/process-missed/21
        //
        // Only ScheduleId = 21 will be processed.
        // =========================================================

        public async Task<PaymentBehaviorLog>
            ProcessMissedPaymentAsync(int scheduleId)
        {
            // Validate ScheduleId
            if (scheduleId <= 0)
            {
                throw new ArgumentException(
                    "Invalid schedule ID.");
            }

            // Get selected repayment schedule
            var schedule =
                await _scheduleRepository
                    .GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                throw new ArgumentException(
                    $"Repayment schedule {scheduleId} was not found.");
            }

            var today = _clock.Today;

            // -----------------------------------------------------
            // Check whether EMI is overdue
            // -----------------------------------------------------

            if (schedule.DueDate >= today)
            {
                throw new InvalidOperationException(
                    "This EMI is not overdue yet.");
            }

            // -----------------------------------------------------
            // Check whether EMI is already fully paid
            // -----------------------------------------------------

            if (schedule.PaidAmount >= schedule.Emiamount)
            {
                throw new InvalidOperationException(
                    "This EMI is already fully paid.");
            }

            // -----------------------------------------------------
            // Check whether behavior log already exists
            // -----------------------------------------------------

            var existingLog =
                await _behaviorRepository
                    .GetByScheduleIdAsync(scheduleId);

            if (existingLog != null)
            {
                return existingLog;
            }

            // -----------------------------------------------------
            // Get loan
            // -----------------------------------------------------

            var loan =
                await _loanRepository
                    .GetByIdAsync(schedule.LoanId);

            if (loan == null)
            {
                throw new ArgumentException(
                    $"Loan {schedule.LoanId} was not found.");
            }

            // -----------------------------------------------------
            // Mark ONLY this EMI as MISSED
            // -----------------------------------------------------

            schedule.Status = "MISSED";

            _logger.LogInformation(
                "Marking EMI as MISSED. " +
                "ScheduleId={ScheduleId}, " +
                "LoanId={LoanId}, " +
                "DueDate={DueDate}, " +
                "Today={Today}",
                schedule.ScheduleId,
                schedule.LoanId,
                schedule.DueDate,
                today);

            // -----------------------------------------------------
            // Create behavior log
            // -----------------------------------------------------

            var behaviorLog =
                await CreateBehaviorLogAsync(
                    schedule,
                    loan);

            _logger.LogInformation(
                "EMI marked as MISSED successfully. " +
                "ScheduleId={ScheduleId}, " +
                "DaysLate={DaysLate}",
                schedule.ScheduleId,
                behaviorLog.DaysLate);

            return behaviorLog;
        }

        // =========================================================
        // CREATE PAYMENT BEHAVIOR LOG
        // =========================================================

        public async Task<PaymentBehaviorLog>
            CreateBehaviorLogAsync(
                RepaymentSchedule schedule,
                LoanAccount loan)
        {
            var today = _clock.Today;

            string paymentStatus;

            // -----------------------------------------------------
            // Fully paid
            // -----------------------------------------------------

            if (schedule.PaidAmount >= schedule.Emiamount)
            {
                paymentStatus = "PAID";
            }

            // -----------------------------------------------------
            // Due date passed and EMI is not fully paid
            // -----------------------------------------------------

            else if (schedule.DueDate < today)
            {
                paymentStatus = "MISSED";
            }

            // -----------------------------------------------------
            // Due date has not arrived
            // -----------------------------------------------------

            else
            {
                paymentStatus = "PENDING";
            }

            // -----------------------------------------------------
            // Calculate days late
            // -----------------------------------------------------

            var daysLate = 0;

            if (paymentStatus == "MISSED")
            {
                daysLate =
                    today.DayNumber -
                    schedule.DueDate.DayNumber;
            }

            // -----------------------------------------------------
            // Create behavior log
            // -----------------------------------------------------

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
    }
}