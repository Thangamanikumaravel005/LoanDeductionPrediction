using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        
        // LOAN OFFICER DASHBOARD
       

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
                    message =
                        "Invalid user identity."
                });
            }

            var dashboard =
                await _dashboardService
                    .GetLoanOfficerDashboardAsync(
                        loanOfficerId);

            return Ok(dashboard);
        }

        
        // ADMIN DASHBOARD
        
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
    }
}