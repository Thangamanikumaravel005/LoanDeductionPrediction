namespace LoanDeductionPrediction.Repositories.Entities
{
    public class LoanHistory
    {
        public int LoanHistoryId { get; set; }

        public int OriginalLoanId { get; set; }

        public int BorrowerId { get; set; }

        public int? LoanOfficerId { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal? InterestRate { get; set; }

        public int? TenureMonths { get; set; }

        public decimal EmiAmount { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public decimal OutstandingAmount { get; set; }

        public string Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
    }
}