using System.ComponentModel.DataAnnotations;

namespace LoanDeductionPrediction.Models.DTOs
{
   public class CreateBorrowerLoanApplicationRequest
{
    [Required(ErrorMessage = "Date of birth is required.")]
    public DateOnly DateOfBirth { get; set; }

    [Range(
        0,
        double.MaxValue,
        ErrorMessage = "Monthly salary cannot be negative.")]
    public decimal? MonthlySalary { get; set; }

    [StringLength(
        500,
        ErrorMessage = "Collateral details cannot exceed 500 characters.")]
    public string? CollateralDetails { get; set; }

    public decimal? CollateralValue { get; set; }

    [Range(
        300,
        850,
        ErrorMessage = "Credit score must be between 300 and 850.")]
    public int? CreditScore { get; set; }

    [Required(ErrorMessage = "Loan type is required.")]
    [StringLength(
        50,
        ErrorMessage = "Loan type cannot exceed 50 characters.")]
    public string LoanType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Requested amount is required.")]
    [Range(
        typeof(decimal),
        "0.01",
        "999999999999999999",
        ErrorMessage = "Requested amount must be greater than zero.")]
    public decimal RequestedAmount { get; set; }
}

    public class BorrowerLoanApplicationDto
    {
        public int ApplicationId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public DateOnly DateOfBirth { get; set; }

        public string Email { get; set; } = string.Empty;

        public decimal? MonthlySalary { get; set; }

        public decimal? CollateralValue { get; set; }

        public int? CreditScore { get; set; }

        public string? CollateralDetails { get; set; }

        public string LoanType { get; set; } = string.Empty;

        public decimal RequestedAmount { get; set; }

        public string Status { get; set; } = string.Empty;

        public int? ReviewedByLoanOfficerId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public decimal? InterestRate { get; set; }

        public int? TenureMonths { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; }
    }


    public class ApproveBorrowerLoanApplicationRequest
    {
        [Required(ErrorMessage = "Interest rate is required.")]
        [Range(
            typeof(decimal),
            "0",
            "100",
            ErrorMessage = "Interest rate must be between 0 and 100.")]
        public decimal InterestRate { get; set; }

        [Required(ErrorMessage = "Tenure in months is required.")]
        [Range(
            1,
            360,
            ErrorMessage = "Tenure must be between 1 and 360 months.")]
        public int TenureMonths { get; set; }
    }


    public class RejectBorrowerLoanApplicationRequest
    {
        [StringLength(
            500,
            ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }
    }


    public class AcceptBorrowerLoanApplicationResponse
    {
        public string Message { get; set; } = string.Empty;

        public BorrowerLoanApplicationDto Application { get; set; } = null!;

        public LoanDto Loan { get; set; } = null!;

        public int BorrowerUserId { get; set; }
    }
}