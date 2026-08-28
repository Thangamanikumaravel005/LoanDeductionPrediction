using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowerLoanApplicationController : ControllerBase
    {
        private readonly IBorrowerLoanApplicationService _applicationService;

        public BorrowerLoanApplicationController(
            IBorrowerLoanApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // =========================================================
        // BORROWER - SUBMIT LOAN APPLICATION
        // POST: api/BorrowerLoanApplication
        // =========================================================
        [AllowAnonymous]
[HttpPost]
        public async Task<IActionResult> SubmitApplication(
            [FromBody] CreateBorrowerLoanApplicationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _applicationService.SubmitApplicationAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.ApplicationId },
                    result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // =========================================================
        // LOAN OFFICER - VIEW PENDING APPLICATIONS
        // GET: api/BorrowerLoanApplication/pending
        // =========================================================
        [HttpGet("pending")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> GetPendingApplications()
        {
            var applications = await _applicationService.GetPendingApplicationsAsync();
            return Ok(applications);
        }

        // =========================================================
        // LOAN OFFICER - VIEW ONE APPLICATION
        // GET: api/BorrowerLoanApplication/{id}
        // =========================================================
        [HttpGet("{id:int}")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> GetById(int id)
        {
            var application = await _applicationService.GetByIdAsync(id);

            if (application == null)
            {
                return NotFound(new
                {
                    message = "Borrower loan application not found."
                });
            }

            return Ok(application);
        }

        // =========================================================
        // BORROWER - VIEW MY APPLICATIONS
        // GET: api/BorrowerLoanApplication/my
        // =========================================================
        [HttpGet("my")]
        [Authorize(Roles = "Borrower")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int borrowerId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var applications = await _applicationService.GetMyApplicationsAsync(borrowerId);
                return Ok(applications);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================================================
        // LOAN OFFICER - ACCEPT APPLICATION
        // POST: api/BorrowerLoanApplication/{id}/accept
        // =========================================================
        [HttpPost("{id:int}/accept")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> AcceptApplication(
            int id,
            [FromBody] ApproveBorrowerLoanApplicationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int loanOfficerId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var response = await _applicationService.AcceptApplicationAsync(
                    id,
                    loanOfficerId,
                    request);

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // =========================================================
        // LOAN OFFICER - REJECT APPLICATION
        // POST: api/BorrowerLoanApplication/{id}/reject
        // =========================================================
        [HttpPost("{id:int}/reject")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> RejectApplication(
            int id,
            [FromBody] RejectBorrowerLoanApplicationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int loanOfficerId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var response = await _applicationService.RejectApplicationAsync(
                    id,
                    loanOfficerId,
                    request);

                return Ok(new
                {
                    message = "Loan application rejected successfully.",
                    application = response
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
