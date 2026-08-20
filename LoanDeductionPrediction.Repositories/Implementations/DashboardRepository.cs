using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.Implementations
{
    public class DashboardRepository
        : IDashboardRepository
    {
        private readonly LoanDeductionDbContext _context;

        public DashboardRepository(
            LoanDeductionDbContext context)
        {
            _context = context;
        }

         
        // LOAN OFFICER - LOANS
         

        public async Task<List<LoanAccount>>
            GetLoansByLoanOfficerIdAsync(
                int loanOfficerId)
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .Where(x =>
                    x.LoanOfficerId ==
                    loanOfficerId)
                .ToListAsync();
        }

         
        // LOAN OFFICER - RISK PREDICTIONS
         

        public async Task<List<RiskPrediction>>
            GetRiskPredictionsByLoanOfficerIdAsync(
                int loanOfficerId)
        {
            var loanIds =
                await _context.LoanAccounts
                    .AsNoTracking()
                    .Where(x =>
                        x.LoanOfficerId ==
                        loanOfficerId)
                    .Select(x => x.LoanId)
                    .ToListAsync();

            if (loanIds.Count == 0)
            {
                return new List<RiskPrediction>();
            }

            return await _context.RiskPredictions
                .AsNoTracking()
                .Where(x =>
                    loanIds.Contains(x.LoanId))
                .ToListAsync();
        }

         
        // LOAN OFFICER - REPAYMENT SCHEDULES
         

        public async Task<List<RepaymentSchedule>>
            GetSchedulesByLoanOfficerIdAsync(
                int loanOfficerId)
        {
            var loanIds =
                await _context.LoanAccounts
                    .AsNoTracking()
                    .Where(x =>
                        x.LoanOfficerId ==
                        loanOfficerId)
                    .Select(x => x.LoanId)
                    .ToListAsync();

            if (loanIds.Count == 0)
            {
                return new List<RepaymentSchedule>();
            }

            return await _context.RepaymentSchedules
                .AsNoTracking()
                .Where(x =>
                    loanIds.Contains(x.LoanId))
                .ToListAsync();
        }

         
        // ADMIN - USERS
         

        public async Task<List<User>>
            GetAllUsersAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .ToListAsync();
        }

         
        // ADMIN - LOANS
         

        public async Task<List<LoanAccount>>
            GetAllLoansAsync()
        {
            return await _context.LoanAccounts
                .AsNoTracking()
                .ToListAsync();
        }

         
        // ADMIN - REPAYMENT SCHEDULES
         

        public async Task<List<RepaymentSchedule>>
            GetAllSchedulesAsync()
        {
            return await _context.RepaymentSchedules
                .AsNoTracking()
                .ToListAsync();
        }

         
        // ADMIN - PAYMENT BEHAVIOR
         

        public async Task<List<PaymentBehaviorLog>>
            GetAllBehaviorLogsAsync()
        {
            return await _context.PaymentBehaviorLogs
                .AsNoTracking()
                .ToListAsync();
        }

         
        // ADMIN - RISK PREDICTIONS
         

        public async Task<List<RiskPrediction>>
            GetAllRiskPredictionsAsync()
        {
            return await _context.RiskPredictions
                .AsNoTracking()
                .ToListAsync();
        }
    }
}