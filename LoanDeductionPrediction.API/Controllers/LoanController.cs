using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        
        // GET: api/Loan Admin and Loan Officer
        

        [HttpGet]
        [Authorize(Roles = "Admin,LoanOfficer")]
        public async Task<IActionResult> GetAll()
        {
            var loans = await _loanService.GetAllAsync();

            return Ok(loans);
        }

        
        // GET: api/Loan/{id} Admin, Loan Officer, Borrower
        

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,LoanOfficer,Borrower")]
        public async Task<IActionResult> GetById(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);

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

            return Ok(loan);
        }

       
        // GET: api/Loan/borrower/{borrowerId} Admin, Loan Officer, Borrower
        

        [HttpGet("borrower/{borrowerId:int}")]
        [Authorize(Roles = "Admin,LoanOfficer,Borrower")]
        public async Task<IActionResult> GetByBorrower(
            int borrowerId)
        {
            var role =
                User.FindFirstValue(ClaimTypes.Role);

            var currentUserId =
                GetCurrentUserId();

            // Borrower can access only their own loans
            if (role == "Borrower" &&
                currentUserId != borrowerId)
            {
                return Forbid();
            }

            var loans =
                await _loanService.GetByBorrowerIdAsync(
                    borrowerId);

            return Ok(loans);
        }

        // GET: api/Loan/officer/{officerId} Admin and Loan Officer
        
        [HttpGet("officer/{officerId:int}")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        public async Task<IActionResult> GetByOfficer(
            int officerId)
        {
            var role =
                User.FindFirstValue(ClaimTypes.Role);

            var currentUserId =
                GetCurrentUserId();

            // Loan Officer can access only their own loans
            if (role == "LoanOfficer" &&
                currentUserId != officerId)
            {
                return Forbid();
            }

            var loans =
                await _loanService.GetByLoanOfficerIdAsync(
                    officerId);

            return Ok(loans);
        }

        
        // POST: api/Loan Admin and Loan Officer

        [HttpPost]
        [Authorize(Roles = "Admin,LoanOfficer")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLoanRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role =
                User.FindFirstValue(ClaimTypes.Role);

            var currentUserId =
                GetCurrentUserId();

            // Loan Officer can create only loans assigned
            // to themselves
            if (role == "LoanOfficer" &&
                currentUserId != request.LoanOfficerId)
            {
                return Forbid();
            }

            try
            {
                var loan =
                    await _loanService.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = loan.LoanId },
                    loan);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        
        // PATCH: api/Loan/{id}/status Admin and Loan Officer
        

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] string status)
        {
            var loan =
                await _loanService.GetByIdAsync(id);

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

            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest(new
                {
                    message = "Loan status is required."
                });
            }

            try
            {
                var updated =
                    await _loanService.UpdateStatusAsync(
                        id,
                        status);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Loan not found."
                    });
                }

                return Ok(new
                {
                    message =
                        "Loan status updated successfully."
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

        
        // AUTHORIZATION HELPER
        

        private bool CanAccessLoan(LoanDto loan)
        {
            var role =
                User.FindFirstValue(ClaimTypes.Role);

            // Admin can access every loan
            if (role == "Admin")
            {
                return true;
            }

            var currentUserId =
                GetCurrentUserId();

            // Loan Officer can access only assigned loans
            if (role == "LoanOfficer")
            {
                return loan.LoanOfficerId ==
                       currentUserId;
            }

            // Borrower can access only their own loans
            if (role == "Borrower")
            {
                return loan.BorrowerId ==
                       currentUserId;
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
    }
}