using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Implementations
{
    public class AlertRepository : IAlertRepository
    {
        private readonly LoanDeductionDbContext _context;

        public AlertRepository(
            LoanDeductionDbContext context)
        {
            _context = context;
        }

         
        // GET ALL ALERTS
         

        public async Task<List<AlertDto>> GetAlertsAsync(
            string role,
            int userId)
        {
            var loansQuery =
                _context.LoanAccounts
                    .AsNoTracking()
                    .AsQueryable();

             
            // ROLE FILTER
             

            if (role == "LoanOfficer")
            {
                loansQuery =
                    loansQuery.Where(
                        x => x.LoanOfficerId == userId);
            }
            else if (role == "Borrower")
            {
                loansQuery =
                    loansQuery.Where(
                        x => x.BorrowerId == userId);
            }
            else if (role != "Admin")
            {
                return new List<AlertDto>();
            }

            var loans =
                await loansQuery.ToListAsync();

            if (loans.Count == 0)
            {
                return new List<AlertDto>();
            }

            var loanIds =
                loans
                    .Select(x => x.LoanId)
                    .ToList();

             
            // LATEST RISK PREDICTION FOR EACH LOAN
             

            var predictions =
                await _context.RiskPredictions
                    .AsNoTracking()
                    .Where(x =>
                        loanIds.Contains(x.LoanId))
                    .ToListAsync();

            var latestPredictions =
                predictions
                    .GroupBy(x => x.LoanId)
                    .Select(group =>
                        group
                            .OrderByDescending(
                                x => x.PredictionDate)
                            .First())
                    .ToList();

             
            // UPCOMING UNPAID INSTALLMENTS
             

            var today =
                DateOnly.FromDateTime(
                    DateTime.Today);

            var upcomingSchedules =
                await _context.RepaymentSchedules
                    .AsNoTracking()
                    .Where(x =>
                        loanIds.Contains(x.LoanId) &&
                        x.DueDate >= today &&
                        x.Status != "PAID")
                    .OrderBy(x => x.DueDate)
                    .ToListAsync();

             
            // CREATE ALERTS
             

            var alerts =
                new List<AlertDto>();

            foreach (var loan in loans)
            {
                var prediction =
                    latestPredictions
                        .FirstOrDefault(
                            x => x.LoanId == loan.LoanId);

                if (prediction == null)
                {
                    continue;
                }

                var nextSchedule =
                    upcomingSchedules
                        .Where(
                            x => x.LoanId == loan.LoanId)
                        .OrderBy(
                            x => x.DueDate)
                        .FirstOrDefault();

                if (nextSchedule == null)
                {
                    continue;
                }

                 
                // HIGH RISK
                 

                if (prediction.RiskLevel == "HIGH")
                {
                    alerts.Add(
                        CreateAlert(
                            loan,
                            nextSchedule,
                            prediction,
                            "HIGH",
                            "HIGH_RISK_UPCOMING_DEDUCTION",
                            "High-risk borrower has an upcoming unpaid deduction."));
                }

                 
                // MEDIUM RISK
                 

                else if (prediction.RiskLevel == "MEDIUM")
                {
                    alerts.Add(
                        CreateAlert(
                            loan,
                            nextSchedule,
                            prediction,
                            "MEDIUM",
                            "MEDIUM_RISK_UPCOMING_DEDUCTION",
                            "Medium-risk borrower has an upcoming unpaid deduction."));
                }
            }

            return alerts;
        }

         
        // GET ALERTS FOR ONE LOAN
         

        public async Task<List<AlertDto>> GetLoanAlertsAsync(
            int loanId)
        {
            var loan =
                await _context.LoanAccounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.LoanId == loanId);

            if (loan == null)
            {
                return new List<AlertDto>();
            }

             
            // LATEST RISK PREDICTION
             

            var prediction =
                await _context.RiskPredictions
                    .AsNoTracking()
                    .Where(
                        x => x.LoanId == loanId)
                    .OrderByDescending(
                        x => x.PredictionDate)
                    .FirstOrDefaultAsync();

            if (prediction == null)
            {
                return new List<AlertDto>();
            }

             
            // NEXT UPCOMING INSTALLMENT
             

            var today =
                DateOnly.FromDateTime(
                    DateTime.Today);

            var nextSchedule =
                await _context.RepaymentSchedules
                    .AsNoTracking()
                    .Where(x =>
                        x.LoanId == loanId &&
                        x.DueDate >= today &&
                        x.Status != "PAID")
                    .OrderBy(
                        x => x.DueDate)
                    .FirstOrDefaultAsync();

            if (nextSchedule == null)
            {
                return new List<AlertDto>();
            }

            var alerts =
                new List<AlertDto>();

            if (prediction.RiskLevel == "HIGH")
            {
                alerts.Add(
                    CreateAlert(
                        loan,
                        nextSchedule,
                        prediction,
                        "HIGH",
                        "HIGH_RISK_UPCOMING_DEDUCTION",
                        "High-risk borrower has an upcoming unpaid deduction."));
            }
            else if (prediction.RiskLevel == "MEDIUM")
            {
                alerts.Add(
                    CreateAlert(
                        loan,
                        nextSchedule,
                        prediction,
                        "MEDIUM",
                        "MEDIUM_RISK_UPCOMING_DEDUCTION",
                        "Medium-risk borrower has an upcoming unpaid deduction."));
            }

            return alerts;
        }

         
        // CREATE ALERT DTO
         

        private static AlertDto CreateAlert(
            LoanAccount loan,
            RepaymentSchedule schedule,
            RiskPrediction prediction,
            string severity,
            string alertType,
            string message)
        {
            return new AlertDto
            {
                AlertType = alertType,

                Severity = severity,

                LoanId = loan.LoanId,

                BorrowerId = loan.BorrowerId,

                ScheduleId =
                    schedule.ScheduleId,

                DueDate =
                    schedule.DueDate,

                EmiAmount =
                    schedule.Emiamount,

                PaidAmount =
                    schedule.PaidAmount,

                RemainingAmount =
                    schedule.Emiamount -
                    schedule.PaidAmount,

                RiskScore =
                    prediction.RiskScore,

                RiskLevel =
                    prediction.RiskLevel,

                Reason =
                    prediction.Reason,

                Message =
                    message
            };
        }
    }
}