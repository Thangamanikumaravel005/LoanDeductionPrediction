using LoanDeductionPrediction.Services.Interfaces;
using LoanDeductionPrediction.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentBehaviorController : ControllerBase
    {
        private readonly IPaymentBehaviorService _behaviorService;
        private readonly ILoanService _loanService;

        public PaymentBehaviorController(
            IPaymentBehaviorService behaviorService,
            ILoanService loanService)
        {
            _behaviorService = behaviorService;
            _loanService = loanService;
        }

        
        // GET: api/PaymentBehavior/borrower/{borrowerId}(borrower can only see their own behavior)
        

        [HttpGet("borrower/{borrowerId:int}")]
        public async Task<IActionResult> GetBorrowerBehavior(
            int borrowerId)
        {
            var role = GetCurrentRole();
            var currentUserId = GetCurrentUserId();

            // Borrower can only see their own behavior.
            if (role == "Borrower" &&
                currentUserId != borrowerId)
            {
                return Forbid();
            }

            if (role != "Admin" &&
                role != "LoanOfficer" &&
                role != "Borrower")
            {
                return Forbid();
            }

            try
            {
                var logs =
                    await _behaviorService
                        .GetByBorrowerIdAsync(borrowerId);

                var result = logs
                    .Select(p => new
                    {
                        p.BehaviorLogId,
                        p.BorrowerId,
                        BorrowerName =
                            p.Borrower?.FullName,

                        p.LoanId,
                        p.ScheduleId,
                        p.DueDate,
                        p.PaymentDate,
                        p.DaysLate,
                        p.PaymentStatus,
                        p.RecordedAt
                    })
                    .ToList();

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message,
                    errorCode = "BORROWER_NOT_FOUND",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        
        // GET: api/PaymentBehavior/loan/{loanId}
        

        [HttpGet("loan/{loanId:int}")]
        public async Task<IActionResult> GetLoanBehavior(
            int loanId)
        {
            var loan =
                await _loanService.GetByIdAsync(loanId);

            if (loan == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Loan not found.",
                    errorCode = "LOAN_NOT_FOUND",
                    traceId = HttpContext.TraceIdentifier
                });
            }

            if (!CanAccessLoan(loan))
            {
                return Forbid();
            }

            var logs =
                await _behaviorService
                    .GetByLoanIdAsync(loanId);

            var result = logs
                .Select(p => new
                {
                    p.BehaviorLogId,
                    p.BorrowerId,
                    p.LoanId,
                    p.ScheduleId,
                    p.DueDate,
                    p.PaymentDate,
                    p.DaysLate,
                    p.PaymentStatus,
                    p.RecordedAt
                })
                .ToList();

            return Ok(result);
        }

      
        // GET: api/PaymentBehavior/{id}
       
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBehavior(
            int id)
        {
            var log =
                await _behaviorService
                    .GetByIdAsync(id);

            if (log == null)
            {
                return NotFound(new
                {
                    success = false,
                    message =
                        "Payment behavior record not found.",
                    errorCode =
                        "PAYMENT_BEHAVIOR_NOT_FOUND",
                    traceId =
                        HttpContext.TraceIdentifier
                });
            }

            var loan =
                await _loanService
                    .GetByIdAsync(log.LoanId);

            if (loan == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Loan not found.",
                    errorCode = "LOAN_NOT_FOUND",
                    traceId = HttpContext.TraceIdentifier
                });
            }

            if (!CanAccessLoan(loan))
            {
                return Forbid();
            }

            return Ok(new
            {
                log.BehaviorLogId,
                log.BorrowerId,
                log.LoanId,
                log.ScheduleId,
                log.DueDate,
                log.PaymentDate,
                log.DaysLate,
                log.PaymentStatus,
                log.RecordedAt
            });
        }

        
        // POST: api/PaymentBehavior/record/{scheduleId}
       

        [HttpPost("record/{scheduleId:int}")]
        [Authorize(Roles = "Admin,Borrower")]
        public async Task<IActionResult> RecordBehavior(
            int scheduleId)
        {
            try
            {
                var behavior =
                    await _behaviorService
                        .RecordBehaviorAsync(scheduleId);

                var loan =
                    await _loanService
                        .GetByIdAsync(behavior.LoanId);

                if (loan == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Loan not found.",
                        errorCode = "LOAN_NOT_FOUND",
                        traceId =
                            HttpContext.TraceIdentifier
                    });
                }

                if (!CanAccessLoan(loan))
                {
                    return Forbid();
                }

                return CreatedAtAction(
                    nameof(GetBehavior),
                    new
                    {
                        id = behavior.BehaviorLogId
                    },
                    new
                    {
                        success = true,

                        message =
                            "Payment behavior recorded successfully.",

                        behavior = new
                        {
                            behavior.BehaviorLogId,
                            behavior.BorrowerId,
                            behavior.LoanId,
                            behavior.ScheduleId,
                            behavior.DueDate,
                            behavior.PaymentDate,
                            behavior.DaysLate,
                            behavior.PaymentStatus,
                            behavior.RecordedAt
                        }
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    errorCode = "VALIDATION_ERROR",
                    traceId = HttpContext.TraceIdentifier
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    success = false,
                    message = ex.Message,
                    errorCode = "BUSINESS_RULE_ERROR",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        
        // GET: api/PaymentBehavior/borrower/{borrowerId}/summary
        
        [HttpGet("borrower/{borrowerId:int}/summary")]
        public async Task<IActionResult> GetBehaviorSummary(
            int borrowerId)
        {
            var role = GetCurrentRole();
            var currentUserId = GetCurrentUserId();

            if (role == "Borrower" &&
                currentUserId != borrowerId)
            {
                return Forbid();
            }

            if (role != "Admin" &&
                role != "LoanOfficer" &&
                role != "Borrower")
            {
                return Forbid();
            }

            try
            {
                var logs =
                    await _behaviorService
                        .GetByBorrowerIdAsync(borrowerId);

                if (!logs.Any())
                {
                    return Ok(new
                    {
                        borrowerId,

                        totalPayments = 0,

                        onTimePayments = 0,

                        latePayments = 0,

                        missedPayments = 0,

                        partialPayments = 0,

                        averageDaysLate = 0
                    });
                }

                return Ok(new
                {
                    borrowerId,

                    totalPayments =
                        logs.Count,

                    onTimePayments =
                        logs.Count(p =>
                            p.PaymentStatus ==
                            "ON_TIME"),

                    latePayments =
                        logs.Count(p =>
                            p.PaymentStatus ==
                            "LATE"),

                    missedPayments =
                        logs.Count(p =>
                            p.PaymentStatus ==
                            "MISSED"),

                    partialPayments =
                        logs.Count(p =>
                            p.PaymentStatus ==
                            "PARTIAL"),

                    averageDaysLate =
                        Math.Round(
                            logs.Average(p =>
                                (double)p.DaysLate),
                            2)
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message,
                    errorCode = "BORROWER_NOT_FOUND",
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        
        // AUTHORIZATION
        

        private bool CanAccessLoan(
            LoanDto loan)
        {
            var role = GetCurrentRole();

            // Admin can access all loans.
            if (role == "Admin")
            {
                return true;
            }

            var userId =
                GetCurrentUserId();

            // Loan Officer can access
            // only assigned loans.
            if (role == "LoanOfficer")
            {
                return loan.LoanOfficerId ==
                       userId;
            }

            // Borrower can access
            // only their own loans.
            if (role == "Borrower")
            {
                return loan.BorrowerId ==
                       userId;
            }

            return false;
        }

        
        // CURRENT USER ID
        

        private int GetCurrentUserId()
        {
            var claim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return int.TryParse(
                claim,
                out var userId)
                ? userId
                : 0;
        }

        
    // CURRENT USER ROLE

        private string? GetCurrentRole()
        {
            return User.FindFirstValue(
                ClaimTypes.Role);
        }
    }
}