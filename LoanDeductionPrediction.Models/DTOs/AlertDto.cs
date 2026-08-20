namespace LoanDeductionPrediction.Models.DTOs
{
    public class AlertDto
    {
        public string AlertType { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public int LoanId { get; set; }

        public int BorrowerId { get; set; }

        public int ScheduleId { get; set; }

        public DateOnly DueDate { get; set; }

        public decimal EmiAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public decimal RiskScore { get; set; }

        public string RiskLevel { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}