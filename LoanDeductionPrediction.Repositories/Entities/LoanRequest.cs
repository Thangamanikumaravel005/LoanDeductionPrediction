using LoanDeductionPrediction.Repositories.Entities;
public class LoanRequest
{
    public int LoanRequestId { get; set; }

    public int BorrowerId { get; set; }

    public decimal RequestedAmount { get; set; }

    public decimal? InterestRate { get; set; }

    public int TenureMonths { get; set; }

    public string Status { get; set; } = "PENDING";

    public DateTime RequestedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public int? ReviewedByLoanOfficerId { get; set; }

    public string? Remarks { get; set; }

    public virtual User Borrower { get; set; } = null!;

    public virtual User? LoanOfficer { get; set; }
}