using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationTestController : ControllerBase
    {
        
        // ANY AUTHENTICATED U
        

        [HttpGet("authenticated")]
        [Authorize]
        public IActionResult Authenticated()
        {
            return Ok(new
            {
                message = "Authenticated user access granted.",
                userId = User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value,
                email = User.FindFirst(
                    ClaimTypes.Email)?.Value,
                role = User.FindFirst(
                    ClaimTypes.Role)?.Value
            });
        }

        // Admin-only endpoint

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return Ok(new
            {
                message = "Admin access granted."
            });
        }

        
        // LOAN OFFICER O
        
        [HttpGet("loan-officer")]
        [Authorize(Roles = "LoanOfficer")]
        public IActionResult LoanOfficer()
        {
            return Ok(new
            {
                message = "Loan Officer access granted."
            });
        }

        

        [HttpGet("borrower")]
        [Authorize(Roles = "Borrower")]
        public IActionResult Borrower()
        {
            return Ok(new
            {
                message = "Borrower access granted."
            });
        }
    }
}