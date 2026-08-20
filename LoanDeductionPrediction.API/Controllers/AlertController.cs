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
    public class AlertController : ControllerBase
    {
        private readonly IAlertService _alertService;
        private readonly ILoanService _loanService;

        public AlertController(
            IAlertService alertService,
            ILoanService loanService)
        {
            _alertService = alertService;
            _loanService = loanService;
        }

        // GET: api/Alert

        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            var role =
                User.FindFirstValue(
                    ClaimTypes.Role);

            var userIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                userIdClaim,
                out int userId))
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid user identity."
                });
            }

            if (role != "Admin" &&
                role != "LoanOfficer" &&
                role != "Borrower")
            {
                return Forbid();
            }

            var alerts =
                await _alertService
                    .GetAlertsAsync(
                        role,
                        userId);

            return Ok(new
            {
                totalAlerts =
                    alerts.Count,

                alerts
            });
        }

        /// GET: api/Alert/loan/{loanId}

        [HttpGet("loan/{loanId:int}")]
        public async Task<IActionResult>
            GetLoanAlerts(int loanId)
        {
            if (loanId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid loan ID."
                });
            }

            var loan =
                await _loanService
                    .GetByIdAsync(loanId);

            if (loan == null)
            {
                return NotFound(new
                {
                    message =
                        "Loan not found."
                });
            }

            if (!CanAccessLoan(loan))
            {
                return Forbid();
            }

            var alerts =
                await _alertService
                    .GetLoanAlertsAsync(loanId);

            return Ok(new
            {
                loanId,

                totalAlerts =
                    alerts.Count,

                alerts
            });
        }

        // AUTHORIZATION

        private bool CanAccessLoan(
            LoanDto loan)
        {
            var role =
                User.FindFirstValue(
                    ClaimTypes.Role);

            // Admin can access all loans.
            if (role == "Admin")
            {
                return true;
            }

            var userId =
                GetCurrentUserId();

            // Loan Officer can access only assigned loans.
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
    }
}