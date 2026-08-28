using LoanDeductionPrediction.Repositories.Entities;

public class LoanRequest
{
    public int LoanRequestId { get; set; }

    public int BorrowerId { get; set; }

    // ================================
    // BORROWER FINANCIAL INFORMATION
    // ================================

    public decimal? MonthlySalary { get; set; }

    public string? CollateralDetails { get; set; }

    public decimal? CollateralValue { get; set; }

    // ================================
    // LOAN REQUEST
    // ================================

    public decimal RequestedAmount { get; set; }

    public string LoanType { get; set; } = string.Empty;

    // ================================
    // LOAN OFFICER DECISION
    // ================================

    public decimal? InterestRate { get; set; }

    public int? TenureMonths { get; set; }

    public string Status { get; set; } = "PENDING";

    public DateTime RequestedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByLoanOfficerId { get; set; }

    public string? Remarks { get; set; }

    // ================================
    // NAVIGATION PROPERTIES
    // ================================

    public virtual User Borrower { get; set; } = null!;

    public virtual User? LoanOfficer { get; set; }
}