namespace LoanDeductionPrediction.Repositories.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int BorrowerId { get; set; }

        public int LoanId { get; set; }

        public int ScheduleId { get; set; }

        public decimal Amount { get; set; }

        public DateOnly PaymentDate { get; set; }

        public string PaymentStatus { get; set; } = "SUCCESS";

        public DateTime CreatedAt { get; set; }


        // ============================================================
        // NAVIGATION PROPERTIES
        // ============================================================

        public User? Borrower { get; set; }

        public LoanAccount? Loan { get; set; }

        public RepaymentSchedule? Schedule { get; set; }
    }
}