using System;
using System.Collections.Generic;

namespace LoanDeductionPrediction.Repositories.Entities;

public partial class RepaymentSchedule
{
    public int ScheduleId { get; set; }

    public int LoanId { get; set; }

    public int InstallmentNumber { get; set; }

    public DateOnly DueDate { get; set; }

    public decimal PrincipalAmount { get; set; }

    public decimal InterestAmount { get; set; }

    public decimal Emiamount { get; set; }

    public decimal PaidAmount { get; set; }

    public DateOnly? PaidDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual LoanAccount Loan { get; set; } = null!;

    public virtual ICollection<PaymentBehaviorLog> PaymentBehaviorLogs { get; set; } = new List<PaymentBehaviorLog>();
}
