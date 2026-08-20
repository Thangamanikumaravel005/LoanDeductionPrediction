namespace LoanDeductionPrediction.Models.DTOs
{
    public class RiskPredictionDto
    {
        public int RiskPredictionId { get; set; }

        public int BorrowerId { get; set; }

        public int LoanId { get; set; }

        public decimal RiskScore { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public DateTime PredictionDate { get; set; }

        public string? Reason { get; set; }
    }
}