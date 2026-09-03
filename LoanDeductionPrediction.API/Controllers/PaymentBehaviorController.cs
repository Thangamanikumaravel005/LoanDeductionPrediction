using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentBehaviorController : ControllerBase
    {
        private readonly IPaymentBehaviorService _service;

        public PaymentBehaviorController(
            IPaymentBehaviorService service)
        {
            _service = service;
        }

        
        // GET BY BORROWER
        

        [HttpGet("borrower/{borrowerId:int}")]
        public async Task<ActionResult<List<PaymentBehaviorLog>>>
            GetByBorrower(int borrowerId)
        {
            var result =
                await _service.GetByBorrowerIdAsync(
                    borrowerId);

            return Ok(result);
        }

        
        // GET BY LOAN
        

        [HttpGet("loan/{loanId:int}")]
        public async Task<ActionResult<List<PaymentBehaviorLog>>>
            GetByLoan(int loanId)
        {
            var result =
                await _service.GetByLoanIdAsync(
                    loanId);

            return Ok(result);
        }

        
        // GET BY ID
        

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PaymentBehaviorLog>>
            GetById(int id)
        {
            var result =
                await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Payment behavior record not found."
                    });
            }

            return Ok(result);
        }

        
// RECORD MISSEDPAYMENT BEHAVIOR

        [HttpPost("process-missed/{scheduleId:int}")]
public async Task<IActionResult> ProcessMissedPayment(
    int scheduleId)
{
    var result =
        await _service.ProcessMissedPaymentAsync(scheduleId);

    return Ok(new
    {
        message = "Selected EMI marked as MISSED.",
        scheduleId = scheduleId,
        paymentStatus = result.PaymentStatus,
        daysLate = result.DaysLate
    });
}
    }
}