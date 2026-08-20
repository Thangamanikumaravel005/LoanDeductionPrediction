using System;
using System.Collections.Generic;

namespace LoanDeductionPrediction.Repositories.Entities;

public partial class LoanAccount
{
    public int LoanId { get; set; }

    public int BorrowerId { get; set; }

    public int LoanOfficerId { get; set; }

    public decimal PrincipalAmount { get; set; }

    public decimal InterestRate { get; set; }

    public int TenureMonths { get; set; }

    public decimal Emiamount { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal OutstandingAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual User Borrower { get; set; } = null!;

    public virtual User LoanOfficer { get; set; } = null!;

    public virtual ICollection<PaymentBehaviorLog> PaymentBehaviorLogs { get; set; } = new List<PaymentBehaviorLog>();

    public virtual ICollection<RepaymentSchedule> RepaymentSchedules { get; set; } = new List<RepaymentSchedule>();

    public virtual ICollection<RiskPrediction> RiskPredictions { get; set; } = new List<RiskPrediction>();
}
