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
    public class RiskPredictionController : ControllerBase
    {
        private readonly IRiskPredictionService _riskService;
        private readonly ILoanService _loanService;

        public RiskPredictionController(
            IRiskPredictionService riskService,
            ILoanService loanService)
        {
            _riskService = riskService;
            _loanService = loanService;
        }

         
        // POST: api/RiskPrediction/generate/{loanId}
         

        [HttpPost("generate/{loanId:int}")]
        [Authorize(Roles = "Admin,LoanOfficer")]
        public async Task<IActionResult> GeneratePrediction(
            int loanId)
        {
            try
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

                var prediction =
                    await _riskService
                        .GeneratePredictionAsync(loanId);

                return CreatedAtAction(
                    nameof(GetPrediction),
                    new
                    {
                        id = prediction.RiskPredictionId
                    },
                    new
                    {
                        message =
                            "Risk prediction generated successfully.",

                        prediction = new
                        {
                            prediction.RiskPredictionId,
                            prediction.BorrowerId,
                            prediction.LoanId,
                            prediction.RiskScore,
                            prediction.RiskLevel,
                            prediction.PredictionDate,
                            prediction.Reason
                        }
                    });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
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

         
        // GET: api/RiskPrediction/{id}
         

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPrediction(
            int id)
        {
            var prediction =
                await _riskService.GetByIdAsync(id);

            if (prediction == null)
            {
                return NotFound(new
                {
                    message =
                        "Risk prediction not found."
                });
            }

            var loan =
                await _loanService
                    .GetByIdAsync(prediction.LoanId);

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

            return Ok(new
            {
                prediction.RiskPredictionId,
                prediction.BorrowerId,
                prediction.LoanId,
                prediction.RiskScore,
                prediction.RiskLevel,
                prediction.PredictionDate,
                prediction.Reason
            });
        }

         
        // GET:
        // api/RiskPrediction/loan/{loanId}
         

        [HttpGet("loan/{loanId:int}")]
        public async Task<IActionResult> GetLoanPredictions(
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

            var predictions =
                await _riskService
                    .GetByLoanIdAsync(loanId);

            return Ok(
                predictions.Select(p => new
                {
                    p.RiskPredictionId,
                    p.BorrowerId,
                    p.LoanId,
                    p.RiskScore,
                    p.RiskLevel,
                    p.PredictionDate,
                    p.Reason
                }));
        }

         
        // GET:
        // api/RiskPrediction/borrower/{borrowerId}
         

        [HttpGet("borrower/{borrowerId:int}")]
        public async Task<IActionResult>
            GetBorrowerPredictions(
                int borrowerId)
        {
            var role =
                User.FindFirstValue(
                    ClaimTypes.Role);

            var currentUserId =
                GetCurrentUserId();

            // Borrower can only see own predictions.
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

            var predictions =
                await _riskService
                    .GetByBorrowerIdAsync(
                        borrowerId);

            // -----------------------------------------------------
            // Borrower
            // -----------------------------------------------------

            if (role == "Borrower")
            {
                return Ok(
                    predictions.Select(p => new
                    {
                        p.RiskPredictionId,
                        p.BorrowerId,
                        p.LoanId,
                        p.RiskScore,
                        p.RiskLevel,
                        p.PredictionDate,
                        p.Reason
                    }));
            }

            // -----------------------------------------------------
            // Admin
            // -----------------------------------------------------

            if (role == "Admin")
            {
                return Ok(
                    predictions.Select(p => new
                    {
                        p.RiskPredictionId,
                        p.BorrowerId,
                        p.LoanId,
                        p.RiskScore,
                        p.RiskLevel,
                        p.PredictionDate,
                        p.Reason
                    }));
            }

            // -----------------------------------------------------
            // Loan Officer
            //
            // Only return predictions belonging to loans
            // assigned to this officer.
            // -----------------------------------------------------

            var officerPredictions =
                new List<object>();

            foreach (var prediction in predictions)
            {
                var loan =
                    await _loanService
                        .GetByIdAsync(
                            prediction.LoanId);

                if (loan != null &&
                    loan.LoanOfficerId ==
                    currentUserId)
                {
                    officerPredictions.Add(
                        new
                        {
                            prediction.RiskPredictionId,
                            prediction.BorrowerId,
                            prediction.LoanId,
                            prediction.RiskScore,
                            prediction.RiskLevel,
                            prediction.PredictionDate,
                            prediction.Reason
                        });
                }
            }

            return Ok(officerPredictions);
        }

         
        // AUTHORIZATION
         

        private bool CanAccessLoan(
            LoanDto loan)
        {
            var role =
                User.FindFirstValue(
                    ClaimTypes.Role);

            // Admin
            if (role == "Admin")
            {
                return true;
            }

            var userId =
                GetCurrentUserId();

            // Loan Officer
            if (role == "LoanOfficer")
            {
                return loan.LoanOfficerId ==
                       userId;
            }

            // Borrower
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