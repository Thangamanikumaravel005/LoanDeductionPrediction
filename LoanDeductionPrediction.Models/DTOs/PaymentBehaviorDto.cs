namespace LoanDeductionPrediction.Models.DTOs
{
    public class PaymentBehaviorDto
    {
        public int BehaviorLogId { get; set; }

        public int BorrowerId { get; set; }

        public int LoanId { get; set; }

        public int ScheduleId { get; set; }

        public DateOnly DueDate { get; set; }

        public DateOnly? PaymentDate { get; set; }

        public int DaysLate { get; set; }

        public string PaymentStatus { get; set; } = string.Empty;

        public DateTime RecordedAt { get; set; }
    }
}