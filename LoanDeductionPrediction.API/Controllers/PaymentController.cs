using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(
            IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // ============================================================
        // PAY ONE MONTH'S EMI
        // ============================================================

        [HttpPost("pay/{scheduleId:int}")]
        public async Task<IActionResult> PayEmi(
            int scheduleId)
        {
            if (scheduleId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid schedule ID."
                });
            }

            var payment =
                await _paymentService
                    .PayEmiAsync(scheduleId);

            return Ok(new
            {
                message =
                    "EMI payment completed successfully.",

                payment = new
                {
                    paymentId =
                        payment.PaymentId,

                    borrowerId =
                        payment.BorrowerId,

                    loanId =
                        payment.LoanId,

                    scheduleId =
                        payment.ScheduleId,

                    amount =
                        payment.Amount,

                    paymentDate =
                        payment.PaymentDate,

                    paymentStatus =
                        payment.PaymentStatus,

                    createdAt =
                        payment.CreatedAt
                }
            });
        }

        // ============================================================
        // GET PAYMENT HISTORY BY LOAN
        // ============================================================

        [HttpGet("loan/{loanId:int}")]
        public async Task<IActionResult> GetByLoan(
            int loanId)
        {
            if (loanId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid loan ID."
                });
            }

            var payments =
                await _paymentService
                    .GetByLoanIdAsync(loanId);

            var response =
                payments.Select(payment => new
                {
                    paymentId =
                        payment.PaymentId,

                    borrowerId =
                        payment.BorrowerId,

                    loanId =
                        payment.LoanId,

                    scheduleId =
                        payment.ScheduleId,

                    amount =
                        payment.Amount,

                    paymentDate =
                        payment.PaymentDate,

                    paymentStatus =
                        payment.PaymentStatus,

                    createdAt =
                        payment.CreatedAt
                });

            return Ok(new
            {
                loanId,

                totalPayments =
                    payments.Count,

                payments = response
            });
        }

        // ============================================================
        // GET PAYMENT HISTORY BY BORROWER
        // ============================================================

        [HttpGet("borrower/{borrowerId:int}")]
        public async Task<IActionResult> GetByBorrower(
            int borrowerId)
        {
            if (borrowerId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid borrower ID."
                });
            }

            var payments =
                await _paymentService
                    .GetByBorrowerIdAsync(
                        borrowerId);

            var response =
                payments.Select(payment => new
                {
                    paymentId =
                        payment.PaymentId,

                    borrowerId =
                        payment.BorrowerId,

                    loanId =
                        payment.LoanId,

                    scheduleId =
                        payment.ScheduleId,

                    amount =
                        payment.Amount,

                    paymentDate =
                        payment.PaymentDate,

                    paymentStatus =
                        payment.PaymentStatus,

                    createdAt =
                        payment.CreatedAt
                });

            return Ok(new
            {
                borrowerId,

                totalPayments =
                    payments.Count,

                payments = response
            });
        }

        // ============================================================
        // GET PAYMENT BY ID
        // ============================================================

        [HttpGet("{paymentId:int}")]
        public async Task<IActionResult> GetById(
            int paymentId)
        {
            if (paymentId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid payment ID."
                });
            }

            var payment =
                await _paymentService
                    .GetByIdAsync(paymentId);

            if (payment == null)
            {
                return NotFound(new
                {
                    message = "Payment not found."
                });
            }

            return Ok(new
            {
                paymentId =
                    payment.PaymentId,

                borrowerId =
                    payment.BorrowerId,

                loanId =
                    payment.LoanId,

                scheduleId =
                    payment.ScheduleId,

                amount =
                    payment.Amount,

                paymentDate =
                    payment.PaymentDate,

                paymentStatus =
                    payment.PaymentStatus,

                createdAt =
                    payment.CreatedAt
            });
        }
    }
}