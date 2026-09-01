using System;

namespace LoanDeductionPrediction.Repositories.Entities
{
    public class BorrowerLoanApplication
{
    public int ApplicationId { get; set; }

    public int BorrowerId { get; set; }

public virtual User Borrower { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public decimal? MonthlySalary { get; set; }

    public string? CollateralDetails { get; set; }

    public decimal? CollateralValue { get; set; }

    public string LoanType { get; set; } = null!;

    public decimal RequestedAmount { get; set; }

    public string Status { get; set; } = "PENDING";

    public int? ReviewedByLoanOfficerId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public decimal? InterestRate { get; set; }

    public int? TenureMonths { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User? ReviewedByLoanOfficer { get; set; }
}
}
