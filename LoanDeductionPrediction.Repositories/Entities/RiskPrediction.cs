namespace LoanDeductionPrediction.Repositories.Entities;

public partial class RiskPrediction
{
    public int RiskPredictionId { get; set; }

    public int BorrowerId { get; set; }

    public int LoanId { get; set; }

    public decimal RiskScore { get; set; }

    public string RiskLevel { get; set; } = null!;

    public DateTime PredictionDate { get; set; }

    public string? Reason { get; set; }

    public virtual User Borrower { get; set; } = null!;

    public virtual LoanAccount Loan { get; set; } = null!;
}
