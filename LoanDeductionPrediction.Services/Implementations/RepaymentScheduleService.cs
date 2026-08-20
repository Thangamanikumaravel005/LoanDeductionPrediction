using AutoMapper;

using LoanDeductionPrediction.Models.DTOs;

using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Repositories.UnitOfWork;

using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class RepaymentScheduleService
        : IRepaymentScheduleService
    {
        private readonly IRepaymentScheduleRepository
            _scheduleRepository;

        private readonly ILoanRepository
            _loanRepository;

        private readonly IMapper
            _mapper;

        private readonly ILoanDeductionUnitOfWork
            _unitOfWork;

        private readonly IPaymentBehaviorService
            _paymentBehaviorService;

         
        // CONSTRUCTOR
         

        public RepaymentScheduleService(
            IRepaymentScheduleRepository scheduleRepository,
            ILoanRepository loanRepository,
            IMapper mapper,
            ILoanDeductionUnitOfWork unitOfWork,
            IPaymentBehaviorService paymentBehaviorService)
        {
            _scheduleRepository =
                scheduleRepository;

            _loanRepository =
                loanRepository;

            _mapper =
                mapper;

            _unitOfWork =
                unitOfWork;

            _paymentBehaviorService =
                paymentBehaviorService;
        }

         
        // GET REPAYMENT SCHEDULE BY LOAN ID
         

        public async Task<List<RepaymentScheduleDto>>
            GetByLoanIdAsync(int loanId)
        {
            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan ID.");
            }

            var schedules =
                await _scheduleRepository
                    .GetByLoanIdAsync(loanId);

            return _mapper.Map<
                List<RepaymentScheduleDto>>(
                schedules);
        }

         
        // GET REPAYMENT SCHEDULE BY ID
         

        public async Task<RepaymentScheduleDto?>
            GetByIdAsync(int scheduleId)
        {
            if (scheduleId <= 0)
            {
                throw new ArgumentException(
                    "Invalid repayment schedule ID.");
            }

            var schedule =
                await _scheduleRepository
                    .GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                return null;
            }

            return _mapper.Map<
                RepaymentScheduleDto>(
                schedule);
        }

         
        // GENERATE REPAYMENT SCHEDULE
         

        public async Task<List<RepaymentScheduleDto>>
            GenerateScheduleAsync(int loanId)
        {
             
            // Validate loan ID
             

            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan ID.");
            }

             
            // Get loan
             

            var loan =
                await _loanRepository
                    .GetByIdAsync(loanId);

            if (loan == null)
            {
                throw new ArgumentException(
                    "Loan not found.");
            }

             
            // Check whether schedule already exists
             

            var exists =
                await _scheduleRepository
                    .ExistsForLoanAsync(loanId);

            if (exists)
            {
                throw new InvalidOperationException(
                    "Repayment schedule already exists for this loan.");
            }

             
            // Validate principal
             

            if (loan.PrincipalAmount <= 0)
            {
                throw new ArgumentException(
                    "Principal amount must be greater than zero.");
            }

             
            // Validate tenure
             

            if (loan.TenureMonths <= 0)
            {
                throw new ArgumentException(
                    "Loan tenure must be greater than zero.");
            }

             
            // Validate interest rate
             

            if (loan.InterestRate < 0)
            {
                throw new ArgumentException(
                    "Interest rate cannot be negative.");
            }

             
            // Validate EMI
             

            if (loan.Emiamount <= 0)
            {
                throw new ArgumentException(
                    "EMI amount must be greater than zero.");
            }

             
            // Create schedule collection
             

            var schedules =
                new List<RepaymentSchedule>();

            decimal remainingPrincipal =
                loan.PrincipalAmount;

             
            // Calculate monthly interest rate
             

            decimal monthlyInterestRate =
                loan.InterestRate /
                12m /
                100m;

             
            // Generate installments
             

            for (
                int installment = 1;
                installment <= loan.TenureMonths;
                installment++)
            {
                 
                // Calculate interest
                 

                decimal interestAmount =
                    Math.Round(
                        remainingPrincipal *
                        monthlyInterestRate,
                        2,
                        MidpointRounding.AwayFromZero);

                 
                // Calculate principal
                 

                decimal principalAmount =
                    loan.Emiamount -
                    interestAmount;

                 
                // Final installment adjustment
                 

                if (
                    installment ==
                    loan.TenureMonths)
                {
                    principalAmount =
                        remainingPrincipal;

                    interestAmount =
                        loan.Emiamount -
                        principalAmount;

                    if (interestAmount < 0)
                    {
                        interestAmount = 0;
                    }
                }

                 
                // Prevent negative principal
                 

                if (principalAmount < 0)
                {
                    principalAmount = 0;
                }

                 
                // Calculate EMI
                 

                decimal emiAmount =
                    principalAmount +
                    interestAmount;

                 
                // Calculate due date
                 

                var dueDate =
                    loan.StartDate.AddMonths(
                        installment);

                 
                // Create repayment schedule
                 

                schedules.Add(
                    new RepaymentSchedule
                    {
                        LoanId =
                            loan.LoanId,

                        InstallmentNumber =
                            installment,

                        DueDate =
                            dueDate,

                        PrincipalAmount =
                            Math.Round(
                                principalAmount,
                                2,
                                MidpointRounding.AwayFromZero),

                        InterestAmount =
                            Math.Round(
                                interestAmount,
                                2,
                                MidpointRounding.AwayFromZero),

                        Emiamount =
                            Math.Round(
                                emiAmount,
                                2,
                                MidpointRounding.AwayFromZero),

                        PaidAmount =
                            0,

                        PaidDate =
                            null,

                        Status =
                            "PENDING"
                    });

                 
                // Reduce remaining principal
                 

                remainingPrincipal -=
                    principalAmount;

                if (remainingPrincipal < 0)
                {
                    remainingPrincipal = 0;
                }
            }

             
            // Save schedules
             

            await _scheduleRepository
                .AddRangeAsync(schedules);

             
            // Return DTOs
             

            return _mapper.Map<
                List<RepaymentScheduleDto>>(
                schedules);
        }

         
        // RECORD PAYMENT
         

        public async Task<RepaymentScheduleDto>
            RecordPaymentAsync(
                int scheduleId,
                decimal paidAmount,
                DateOnly paymentDate)
        {
             
            // VALIDATE SCHEDULE ID
             

            if (scheduleId <= 0)
            {
                throw new ArgumentException(
                    "Invalid repayment schedule ID.");
            }

             
            // VALIDATE PAYMENT AMOUNT
             

            if (paidAmount <= 0)
            {
                throw new ArgumentException(
                    "Payment amount must be greater than zero.");
            }

             
            // VALIDATE PAYMENT DATE
             

            var today =
                DateOnly.FromDateTime(
                    DateTime.Today);

            if (paymentDate > today)
            {
                throw new ArgumentException(
                    "Payment date cannot be in the future.");
            }

             
            // GET REPAYMENT SCHEDULE
             

            var schedule =
                await _scheduleRepository
                    .GetByIdAsync(scheduleId);

            if (schedule == null)
            {
                throw new ArgumentException(
                    "Repayment schedule not found.");
            }

             
            // GET LOAN
             

            var loan =
                await _loanRepository
                    .GetByIdAsync(
                        schedule.LoanId);

            if (loan == null)
            {
                throw new ArgumentException(
                    "Loan not found.");
            }

             
            // VALIDATE LOAN STATUS
             

            if (string.Equals(
                    loan.Status,
                    "CLOSED",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Payment cannot be recorded for a closed loan.");
            }

             
            // VALIDATE EMI
             

            if (schedule.Emiamount <= 0)
            {
                throw new InvalidOperationException(
                    "Invalid installment amount.");
            }

             
            // CHECK WHETHER ALREADY PAID
             

            if (string.Equals(
                    schedule.Status,
                    "PAID",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Payment cannot be recorded because this installment is already paid.");
            }

             
            // CALCULATE REMAINING INSTALLMENT
             

            decimal remainingInstallment =
                schedule.Emiamount -
                schedule.PaidAmount;

            remainingInstallment =
                Math.Round(
                    remainingInstallment,
                    2,
                    MidpointRounding.AwayFromZero);

             
            // VALIDATE REMAINING AMOUNT
             

            if (remainingInstallment <= 0)
            {
                throw new InvalidOperationException(
                    "This installment has no remaining amount to pay.");
            }

             
            // PREVENT OVERPAYMENT
             

            if (paidAmount >
                remainingInstallment)
            {
                throw new ArgumentException(
                    $"Payment amount cannot exceed the remaining amount of {remainingInstallment:0.00}.");
            }

             
            // EXECUTE COMPLETE PAYMENT TRANSACTION
             

            await _unitOfWork
                .ExecuteInTransactionAsync(
                    async () =>
                    {
                         
                        // UPDATE PAID AMOUNT
                         

                        schedule.PaidAmount +=
                            paidAmount;

                        schedule.PaidAmount =
                            Math.Round(
                                schedule.PaidAmount,
                                2,
                                MidpointRounding.AwayFromZero);

                         
                        // UPDATE PAYMENT DATE
                         

                        schedule.PaidDate =
                            paymentDate;

                         
                        // UPDATE PAYMENT STATUS
                         

                        if (
                            schedule.PaidAmount >=
                            schedule.Emiamount)
                        {
                            schedule.PaidAmount =
                                schedule.Emiamount;

                            schedule.Status =
                                "PAID";
                        }
                        else
                        {
                            schedule.Status =
                                "PARTIAL";
                        }

                         
                        // UPDATE LOAN OUTSTANDING BALANCE
                         

                        loan.OutstandingAmount -=
                            paidAmount;

                        if (
                            loan.OutstandingAmount < 0)
                        {
                            loan.OutstandingAmount =
                                0;
                        }

                        loan.OutstandingAmount =
                            Math.Round(
                                loan.OutstandingAmount,
                                2,
                                MidpointRounding.AwayFromZero);

                         
                        // CLOSE LOAN IF FULLY PAID
                         

                        if (
                            loan.OutstandingAmount == 0)
                        {
                            loan.Status =
                                "CLOSED";
                        }

                         
                        // UPDATE REPAYMENT SCHEDULE
                         

                        await _scheduleRepository
                            .UpdateAsync(schedule);

                         
                        // UPDATE LOAN
                         

                        await _loanRepository
                            .UpdateAsync(loan);

                         
                        // CREATE PAYMENT BEHAVIOR LOG
                         

                        await _paymentBehaviorService
                            .CreateBehaviorLogAsync(
                                schedule,
                                loan);
                    });

             
            // RETURN UPDATED SCHEDULE
             

            return _mapper.Map<
                RepaymentScheduleDto>(
                schedule);
        }
    }
}