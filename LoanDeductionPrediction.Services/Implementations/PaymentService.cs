using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRepaymentScheduleRepository _scheduleRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly IPaymentBehaviorService _paymentBehaviorService;
        private readonly IClock _clock;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IRepaymentScheduleRepository scheduleRepository,
            ILoanRepository loanRepository,
            IPaymentBehaviorService paymentBehaviorService,
            IClock clock)
        {
            _paymentRepository = paymentRepository;
            _scheduleRepository = scheduleRepository;
            _loanRepository = loanRepository;
            _paymentBehaviorService = paymentBehaviorService;
            _clock = clock;
        }

        // ============================================================
        // PAY ONE MONTH'S EMI
        // ============================================================

        public async Task<Payment> PayEmiAsync(
            int scheduleId)
        {
            if (scheduleId <= 0)
            {
                throw new ArgumentException(
                    "Invalid schedule ID.");
            }

            // --------------------------------------------------------
            // 1. Find the repayment schedule
            // --------------------------------------------------------

            var schedule =
                await _scheduleRepository
                    .GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                throw new ArgumentException(
                    "Repayment schedule not found.");
            }

            // --------------------------------------------------------
            // 2. Prevent duplicate payment
            // --------------------------------------------------------

            if (string.Equals(
                    schedule.Status,
                    "PAID",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This EMI has already been paid.");
            }

            // --------------------------------------------------------
            // 3. Get the loan
            // --------------------------------------------------------

            var loan =
                await _loanRepository
                    .GetByIdAsync(schedule.LoanId);

            if (loan == null)
            {
                throw new ArgumentException(
                    "Loan not found.");
            }

            // --------------------------------------------------------
            // 4. Calculate remaining EMI amount
            // --------------------------------------------------------

            var remainingAmount =
                schedule.Emiamount -
                schedule.PaidAmount;

            if (remainingAmount <= 0)
            {
                throw new InvalidOperationException(
                    "There is no remaining amount to pay.");
            }

            // --------------------------------------------------------
            // 5. Create payment record
            // --------------------------------------------------------

            var payment = new Payment
            {
                BorrowerId =
                    loan.BorrowerId,

                LoanId =
                    loan.LoanId,

                ScheduleId =
                    schedule.ScheduleId,

                Amount =
                    remainingAmount,

                PaymentDate =
                    _clock.Today,

                PaymentStatus =
                    "SUCCESS",

                CreatedAt =
                    DateTime.UtcNow
            };

            var savedPayment =
                await _paymentRepository
                    .AddAsync(payment);

            // --------------------------------------------------------
            // 6. Update repayment schedule
            // --------------------------------------------------------

            schedule.PaidAmount =
                schedule.PaidAmount +
                remainingAmount;

            schedule.PaidDate =
                _clock.Today;

            schedule.Status =
                "PAID";

            await _scheduleRepository
                .UpdateAsync(schedule);

            // --------------------------------------------------------
            // 7. Record payment behavior
            // --------------------------------------------------------

            await _paymentBehaviorService
                .RecordBehaviorAsync(
                    schedule.ScheduleId);

            return savedPayment;
        }

        // ============================================================
        // GET PAYMENTS BY LOAN
        // ============================================================

        public async Task<List<Payment>>
            GetByLoanIdAsync(
                int loanId)
        {
            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan ID.");
            }

            return await _paymentRepository
                .GetByLoanIdAsync(loanId);
        }

        // ============================================================
        // GET PAYMENTS BY BORROWER
        // ============================================================

        public async Task<List<Payment>>
            GetByBorrowerIdAsync(
                int borrowerId)
        {
            if (borrowerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid borrower ID.");
            }

            return await _paymentRepository
                .GetByBorrowerIdAsync(borrowerId);
        }

        // ============================================================
        // GET PAYMENT BY ID
        // ============================================================

        public async Task<Payment?>
            GetByIdAsync(
                int paymentId)
        {
            if (paymentId <= 0)
            {
                return null;
            }

            return await _paymentRepository
                .GetByIdAsync(paymentId);
        }
    }
}