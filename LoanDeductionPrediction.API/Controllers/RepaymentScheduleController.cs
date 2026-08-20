using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RepaymentScheduleController : ControllerBase
    {
        private readonly IRepaymentScheduleService _scheduleService;
        private readonly ILoanService _loanService;

        public RepaymentScheduleController(
            IRepaymentScheduleService scheduleService,
            ILoanService loanService)
        {
            _scheduleService = scheduleService;
            _loanService = loanService;
        }

        
        // GET: api/RepaymentSchedule/loan/1
        

        [HttpGet("loan/{loanId:int}")]
        public async Task<IActionResult> GetLoanSchedule(
            int loanId)
        {
            var loan =
                await _loanService.GetByIdAsync(loanId);

            if (loan == null)
            {
                return NotFound(new
                {
                    message = "Loan not found."
                });
            }

            if (!CanAccessLoan(loan))
            {
                return Forbid();
            }

            var schedules =
                await _scheduleService
                    .GetByLoanIdAsync(loanId);

            return Ok(schedules);
        }

        
        // GET: api/RepaymentSchedule/1
        

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSchedule(
            int id)
        {
            var schedule =
                await _scheduleService
                    .GetByIdAsync(id);

            if (schedule == null)
            {
                return NotFound(new
                {
                    message =
                        "Repayment schedule not found."
                });
            }

            var loan =
                await _loanService
                    .GetByIdAsync(schedule.LoanId);

            if (loan == null)
            {
                return NotFound(new
                {
                    message = "Loan not found."
                });
            }

            if (!CanAccessLoan(loan))
            {
                return Forbid();
            }

            return Ok(schedule);
        }

        
        // POST: api/RepaymentSchedule/generate/1
        

        [HttpPost("generate/{loanId:int}")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        public async Task<IActionResult> GenerateSchedule(
            int loanId)
        {
            var loan =
                await _loanService
                    .GetByIdAsync(loanId);

            if (loan == null)
            {
                return NotFound(new
                {
                    message = "Loan not found."
                });
            }

            if (!CanAccessLoan(loan))
            {
                return Forbid();
            }

            try
            {
                var schedules =
                    await _scheduleService
                        .GenerateScheduleAsync(loanId);

                return Ok(new
                {
                    message =
                        "Repayment schedule generated successfully.",

                    loanId,

                    installmentCount =
                        schedules.Count,

                    emiAmount = loan.EmiAmount,

                    schedules
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        
        // GET: api/RepaymentSchedule/loan/1/summary
       

        [HttpGet("loan/{loanId:int}/summary")]
        public async Task<IActionResult> GetSummary(
            int loanId)
        {
            var loan =
                await _loanService
                    .GetByIdAsync(loanId);

            if (loan == null)
            {
                return NotFound(new
                {
                    message = "Loan not found."
                });
            }

            if (!CanAccessLoan(loan))
            {
                return Forbid();
            }

            var schedules =
                await _scheduleService
                    .GetByLoanIdAsync(loanId);

            return Ok(new
            {
                loanId,

                totalInstallments =
                    schedules.Count,

                paidInstallments =
                    schedules.Count(
                        x => x.Status == "PAID"),

                pendingInstallments =
                    schedules.Count(
                        x => x.Status == "PENDING"),

                partialInstallments =
                    schedules.Count(
                        x => x.Status == "PARTIAL"),

                totalAmount =
                    schedules.Sum(
                        x => x.EmiAmount),

                totalPaid =
                    schedules.Sum(
                        x => x.PaidAmount),

                totalRemaining =
                    schedules.Sum(
                        x => x.EmiAmount -
                             x.PaidAmount)
            });
        }
         
// PUT: api/RepaymentSchedule/{id}/payment
 

[HttpPut("{id:int}/payment")]
[Authorize(Roles = "Admin,LoanOfficer")]
public async Task<IActionResult> RecordPayment(
    int id,
    [FromBody] RecordPaymentRequest request)
{
    var schedule =
        await _scheduleService.GetByIdAsync(id);

    if (schedule == null)
    {
        return NotFound(new
        {
            message = "Repayment schedule not found."
        });
    }

    var loan =
        await _loanService.GetByIdAsync(schedule.LoanId);

    if (loan == null)
    {
        return NotFound(new
        {
            message = "Loan not found."
        });
    }

    if (!CanAccessLoan(loan))
    {
        return Forbid();
    }

    try
    {
        var result =
            await _scheduleService.RecordPaymentAsync(
                id,
                request.PaidAmount,
                request.PaymentDate);

        return Ok(new
        {
            message = "Payment recorded successfully.",
            schedule = result
        });
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new
        {
            message = ex.Message
        });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(new
        {
            message = ex.Message
        });
    }
}

         
        // AUTHORIZATION
         

        private bool CanAccessLoan(
            LoanDto loan)
        {
            var role =
                User.FindFirstValue(
                    ClaimTypes.Role);

            if (role == "Admin")
            {
                return true;
            }

            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                userIdClaim,
                out int userId))
            {
                return false;
            }

            if (role == "LoanOfficer")
            {
                return loan.LoanOfficerId == userId;
            }

            if (role == "Borrower")
            {
                return loan.BorrowerId == userId;
            }

            return false;
        }
    }
}