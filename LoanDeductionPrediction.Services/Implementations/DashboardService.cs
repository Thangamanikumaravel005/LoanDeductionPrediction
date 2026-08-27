using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly LoanDeductionDbContext _context;

        public DashboardService(
            LoanDeductionDbContext context)
        {
            _context = context;
        }

             
        // LOAN OFFICER DASHBOARD
             

        public async Task<object>
            GetLoanOfficerDashboardAsync(
                int loanOfficerId)
        {
            var loans = await _context.LoanAccounts
                .AsNoTracking()
                .Where(x =>
                    x.LoanOfficerId == loanOfficerId)
                .ToListAsync();

            var loanIds = loans
                .Select(x => x.LoanId)
                .ToList();

            var riskPredictions =
                await _context.RiskPredictions
                    .AsNoTracking()
                    .Where(x =>
                        loanIds.Contains(x.LoanId))
                    .ToListAsync();

            var schedules =
                await _context.RepaymentSchedules
                    .AsNoTracking()
                    .Where(x =>
                        loanIds.Contains(x.LoanId))
                    .ToListAsync();

            var behaviors =
                await _context.PaymentBehaviorLogs
                    .AsNoTracking()
                    .Where(x =>
                        loanIds.Contains(x.LoanId))
                    .ToListAsync();

            return new
            {
                loanOfficerId,

                totalLoans = loans.Count,

                activeLoans = loans.Count(
                    x => x.Status == "ACTIVE"),

                closedLoans = loans.Count(
                    x => x.Status == "CLOSED"),

                pendingLoans = loans.Count(
                    x => x.Status == "PENDING"),

                defaultedLoans = loans.Count(
                    x => x.Status == "DEFAULTED"),

                totalPrincipal = loans.Sum(
                    x => x.PrincipalAmount),

                totalOutstanding = loans.Sum(
                    x => x.OutstandingAmount),

                totalEmiAmount = loans.Sum(
                    x => x.Emiamount),

                totalInstallments = schedules.Count,

                paidInstallments = schedules.Count(
                    x => x.Status == "PAID"),

                pendingInstallments = schedules.Count(
                    x => x.Status == "PENDING"),

                partialInstallments = schedules.Count(
                    x => x.Status == "PARTIAL"),

                totalPaidAmount = schedules.Sum(
                    x => x.PaidAmount),

                totalBehaviorLogs = behaviors.Count,

                onTimePayments = behaviors.Count(
                    x => x.PaymentStatus == "ON_TIME"),

                latePayments = behaviors.Count(
                    x => x.PaymentStatus == "LATE"),

                missedPayments = behaviors.Count(
                    x => x.PaymentStatus == "MISSED"),

                partialPayments = behaviors.Count(
                    x => x.PaymentStatus == "PARTIAL"),

                lowRiskLoans = riskPredictions
                    .GroupBy(x => x.LoanId)
                    .Select(x => x
                        .OrderByDescending(r =>
                            r.PredictionDate)
                        .First())
                    .Count(x =>
                        x.RiskLevel == "LOW"),

                mediumRiskLoans = riskPredictions
                    .GroupBy(x => x.LoanId)
                    .Select(x => x
                        .OrderByDescending(r =>
                            r.PredictionDate)
                        .First())
                    .Count(x =>
                        x.RiskLevel == "MEDIUM"),

                highRiskLoans = riskPredictions
                    .GroupBy(x => x.LoanId)
                    .Select(x => x
                        .OrderByDescending(r =>
                            r.PredictionDate)
                        .First())
                    .Count(x =>
                        x.RiskLevel == "HIGH")
            };
        }


             
        // ADMIN DASHBOARD
             

        public async Task<object>
            GetAdminDashboardAsync()
        {
            var users =
                await _context.Users
                    .AsNoTracking()
                    .ToListAsync();

            var loans =
                await _context.LoanAccounts
                    .AsNoTracking()
                    .ToListAsync();

            var schedules =
                await _context.RepaymentSchedules
                    .AsNoTracking()
                    .ToListAsync();

            var behaviors =
                await _context.PaymentBehaviorLogs
                    .AsNoTracking()
                    .ToListAsync();

            var riskPredictions =
                await _context.RiskPredictions
                    .AsNoTracking()
                    .ToListAsync();

            var latestRiskPredictions =
                riskPredictions
                    .GroupBy(x => x.LoanId)
                    .Select(x =>
                        x.OrderByDescending(r =>
                            r.PredictionDate)
                         .First())
                    .ToList();

            return new
            {
                totalUsers = users.Count,

                totalBorrowers = users.Count(
                    x => x.Role == "Borrower"),

                totalLoanOfficers = users.Count(
                    x => x.Role == "LoanOfficer"),

                totalAdmins = users.Count(
                    x => x.Role == "Admin"),

                totalLoans = loans.Count,

                activeLoans = loans.Count(
                    x => x.Status == "ACTIVE"),

                closedLoans = loans.Count(
                    x => x.Status == "CLOSED"),

                pendingLoans = loans.Count(
                    x => x.Status == "PENDING"),

                defaultedLoans = loans.Count(
                    x => x.Status == "DEFAULTED"),

                totalPrincipal = loans.Sum(
                    x => x.PrincipalAmount),

                totalOutstanding = loans.Sum(
                    x => x.OutstandingAmount),

                totalEmiAmount = loans.Sum(
                    x => x.Emiamount),

                totalInstallments = schedules.Count,

                paidInstallments = schedules.Count(
                    x => x.Status == "PAID"),

                pendingInstallments = schedules.Count(
                    x => x.Status == "PENDING"),

                partialInstallments = schedules.Count(
                    x => x.Status == "PARTIAL"),

                totalPaidAmount = schedules.Sum(
                    x => x.PaidAmount),

                totalBehaviorLogs = behaviors.Count,

                onTimePayments = behaviors.Count(
                    x => x.PaymentStatus == "ON_TIME"),

                latePayments = behaviors.Count(
                    x => x.PaymentStatus == "LATE"),

                missedPayments = behaviors.Count(
                    x => x.PaymentStatus == "MISSED"),

                partialPayments = behaviors.Count(
                    x => x.PaymentStatus == "PARTIAL"),

                lowRiskLoans = latestRiskPredictions.Count(
                    x => x.RiskLevel == "LOW"),

                mediumRiskLoans = latestRiskPredictions.Count(
                    x => x.RiskLevel == "MEDIUM"),

                highRiskLoans = latestRiskPredictions.Count(
                    x => x.RiskLevel == "HIGH")
            };
        }


             
        // BORROWER DASHBOARD
             

        public async Task<object>
            GetBorrowerDashboardAsync(
                int borrowerId)
        {
               
            // STEP 1: Get borrower's loans
               

            var loans =
                await _context.LoanAccounts
                    .AsNoTracking()
                    .Where(x =>
                        x.BorrowerId == borrowerId)
                    .ToListAsync();


               
            // STEP 2: Get loan IDs
               

            var loanIds =
                loans
                    .Select(x => x.LoanId)
                    .ToList();


               
            // STEP 3: Get repayment schedules
               

            var schedules =
                await _context.RepaymentSchedules
                    .AsNoTracking()
                    .Where(x =>
                        loanIds.Contains(x.LoanId))
                    .ToListAsync();


               
            // STEP 4: Get payment behavior
               

            var behaviors =
                await _context.PaymentBehaviorLogs
                    .AsNoTracking()
                    .Where(x =>
                        x.BorrowerId == borrowerId)
                    .ToListAsync();


               
            // STEP 5: Get risk predictions
               

            var riskPredictions =
                await _context.RiskPredictions
                    .AsNoTracking()
                    .Where(x =>
                        x.BorrowerId == borrowerId)
                    .ToListAsync();


               
            // STEP 6: Get latest risk prediction for each loan
               

            var latestRiskPredictions =
                riskPredictions
                    .GroupBy(x => x.LoanId)
                    .Select(x =>
                        x.OrderByDescending(r =>
                            r.PredictionDate)
                         .First())
                    .ToList();


               
            // STEP 7: Return borrower dashboard
               

            return new
            {
                borrowerId,

                  
                // LOAN SUMMARY
                  

                totalLoans = loans.Count,

                activeLoans = loans.Count(
                    x => x.Status == "ACTIVE"),

                closedLoans = loans.Count(
                    x => x.Status == "CLOSED"),

                pendingLoans = loans.Count(
                    x => x.Status == "PENDING"),

                defaultedLoans = loans.Count(
                    x => x.Status == "DEFAULTED"),

                totalPrincipal = loans.Sum(
                    x => x.PrincipalAmount),

                totalOutstanding = loans.Sum(
                    x => x.OutstandingAmount),

                totalEmiAmount = loans.Sum(
                    x => x.Emiamount),


                  
                // REPAYMENT SUMMARY
                  

                totalInstallments =
                    schedules.Count,

                paidInstallments =
                    schedules.Count(
                        x => x.Status == "PAID"),

                pendingInstallments =
                    schedules.Count(
                        x => x.Status == "PENDING"),

                partialInstallments =
                    schedules.Count(
                        x => x.Status == "PARTIAL"),

                totalPaidAmount =
                    schedules.Sum(
                        x => x.PaidAmount),


                  
                // PAYMENT BEHAVIOR
                  

                totalBehaviorLogs =
                    behaviors.Count,

                onTimePayments =
                    behaviors.Count(
                        x => x.PaymentStatus == "ON_TIME"),

                latePayments =
                    behaviors.Count(
                        x => x.PaymentStatus == "LATE"),

                missedPayments =
                    behaviors.Count(
                        x => x.PaymentStatus == "MISSED"),

                partialPayments =
                    behaviors.Count(
                        x => x.PaymentStatus == "PARTIAL"),


                  
                // REPAYMENT SCHEDULE
                  

                repaymentSchedule =
                    schedules
                        .OrderBy(x => x.DueDate)
                        .Select(x => new
                        {
                            x.ScheduleId,

                            x.LoanId,

                            x.InstallmentNumber,

                            x.DueDate,

                            x.PrincipalAmount,

                            x.InterestAmount,

                            EmiAmount =
                                x.Emiamount,

                            x.PaidAmount,

                            x.PaidDate,

                            x.Status
                        })
                        .ToList(),


                  
                // RISK INFORMATION
                  

                riskPredictions =
                    latestRiskPredictions
                        .Select(x => new
                        {
                            x.RiskPredictionId,

                            x.LoanId,

                            x.RiskScore,

                            x.RiskLevel,

                            x.PredictionDate,

                            x.Reason
                        })
                        .OrderByDescending(
                            x => x.PredictionDate)
                        .ToList()
            };
        }
    }
}