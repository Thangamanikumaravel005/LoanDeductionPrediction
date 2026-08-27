using System.Security.Claims;
using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        // =========================================================
        // LOAN OFFICER DASHBOARD
        // =========================================================

        // GET: api/Dashboard/loan-officer

        [HttpGet("loan-officer")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult>
            GetLoanOfficerDashboard()
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

            var dashboard =
                await _dashboardService
                    .GetLoanOfficerDashboardAsync(
                        loanOfficerId);

            return Ok(dashboard);
        }


        // =========================================================
        // ADMIN DASHBOARD
        // =========================================================

        // GET: api/Dashboard/admin

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult>
            GetAdminDashboard()
        {
            var dashboard =
                await _dashboardService
                    .GetAdminDashboardAsync();

            return Ok(dashboard);
        }


        // =========================================================
        // BORROWER DASHBOARD
        // =========================================================

        // GET: api/Dashboard/borrower

        [HttpGet("borrower")]
        [Authorize(Roles = "Borrower")]
        public async Task<IActionResult>
            GetBorrowerDashboard()
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

            var dashboard =
                await _dashboardService
                    .GetBorrowerDashboardAsync(
                        borrowerId);

            return Ok(dashboard);
        }
    }
}