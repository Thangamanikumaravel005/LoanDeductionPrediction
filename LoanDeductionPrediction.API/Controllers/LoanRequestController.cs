using System.Security.Claims;
using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoanRequestController : ControllerBase
    {
        private readonly ILoanRequestService _loanRequestService;

        public LoanRequestController(
            ILoanRequestService loanRequestService)
        {
            _loanRequestService = loanRequestService;
        }


            
        // BORROWER - CREATE LOAN REQUEST
            

        // POST: api/LoanRequest

        [HttpPost]
        [Authorize(Roles = "Borrower")]
        public async Task<IActionResult> CreateRequest(
            [FromBody] CreateLoanRequestDto request)
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                userIdClaim,
                out int borrowerId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var loanRequest =
                    await _loanRequestService
                        .CreateRequestAsync(
                            borrowerId,
                            request);

                return CreatedAtAction(
                    nameof(GetById),
                    new
                    {
                        id = loanRequest.LoanRequestId
                    },
                    new
                    {
                        message =
                            "Loan request submitted successfully.",

                        loanRequestId =
                            loanRequest.LoanRequestId,

                        status =
                            loanRequest.Status,

                        requestedAmount =
                            loanRequest.RequestedAmount,

                        interestRate =
                            loanRequest.InterestRate,

                        tenureMonths =
                            loanRequest.TenureMonths,

                        requestedAt =
                            loanRequest.RequestedAt
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


            
        // BORROWER - VIEW MY REQUESTS
            

        // GET: api/LoanRequest/my

        [HttpGet("my")]
        [Authorize(Roles = "Borrower")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                userIdClaim,
                out int borrowerId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var requests =
                await _loanRequestService
                    .GetMyRequestsAsync(
                        borrowerId);

            return Ok(requests);
        }


            
        // LOAN OFFICER - VIEW PENDING REQUESTS
            

        // GET: api/LoanRequest/pending

        [HttpGet("pending")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult>
            GetPendingRequests()
        {
            var requests =
                await _loanRequestService
                    .GetPendingRequestsAsync();

            return Ok(requests);
        }


            
        // LOAN OFFICER - VIEW ONE REQUEST
            

        // GET: api/LoanRequest/{id}

        [HttpGet("{id:int}")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult>
            GetById(int id)
        {
            var request =
                await _loanRequestService
                    .GetByIdAsync(id);

            if (request == null)
            {
                return NotFound(new
                {
                    message = "Loan request not found."
                });
            }

            return Ok(request);
        }


            
        // LOAN OFFICER - APPROVE REQUEST
            

        // POST: api/LoanRequest/{id}/approve

        [HttpPost("{id:int}/approve")]
[Authorize(Roles = "LoanOfficer")]
public async Task<IActionResult> ApproveRequest(
    int id,
    [FromBody] ApproveLoanRequestDto request)
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                userIdClaim,
                out int loanOfficerId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var result =
    await _loanRequestService
        .ApproveRequestAsync(
            id,
            loanOfficerId,
            request.InterestRate);

                if (!result)
                {
                    return NotFound(new
                    {
                        message =
                            "Loan request not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Loan request approved successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


            
        // LOAN OFFICER - REJECT REQUEST
            

        // POST: api/LoanRequest/{id}/reject

        [HttpPost("{id:int}/reject")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult>
            RejectRequest(
                int id,
                [FromBody] RejectLoanRequestDto request)
        {
            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                userIdClaim,
                out int loanOfficerId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var result =
                    await _loanRequestService
                        .RejectRequestAsync(
                            id,
                            loanOfficerId,
                            request.Remarks);

                if (!result)
                {
                    return NotFound(new
                    {
                        message =
                            "Loan request not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Loan request rejected successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }


        
    // REJECT REQUEST DTO
        

    public class RejectLoanRequestDto
    {
        public string? Remarks { get; set; }
    }
}