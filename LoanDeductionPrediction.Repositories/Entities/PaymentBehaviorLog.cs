namespace LoanDeductionPrediction.Repositories.Entities;

public partial class PaymentBehaviorLog
{
    public int BehaviorLogId { get; set; }

    public int BorrowerId { get; set; }

    public int LoanId { get; set; }

    public int ScheduleId { get; set; }

    public DateOnly DueDate { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public int DaysLate { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime RecordedAt { get; set; }

    public virtual User Borrower { get; set; } = null!;

    public virtual LoanAccount Loan { get; set; } = null!;

    public virtual RepaymentSchedule Schedule { get; set; } = null!;
}
