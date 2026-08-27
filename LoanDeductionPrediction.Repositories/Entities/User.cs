using System;
using System.Collections.Generic;

namespace LoanDeductionPrediction.Repositories.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

     
    // LOAN RELATIONSHIPS
     

    public virtual ICollection<LoanAccount> LoanAccountBorrowers { get; set; }
    = new List<LoanAccount>();

public virtual ICollection<LoanAccount> LoanAccountLoanOfficers { get; set; }
    = new List<LoanAccount>();

public virtual ICollection<PaymentBehaviorLog> PaymentBehaviorLogs { get; set; }
    = new List<PaymentBehaviorLog>();

public virtual ICollection<RiskPrediction> RiskPredictions { get; set; }
    = new List<RiskPrediction>();

public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    = new List<RefreshToken>();

public virtual ICollection<BorrowerLoanApplication> ReviewedBorrowerApplications { get; set; }
    = new List<BorrowerLoanApplication>();
}