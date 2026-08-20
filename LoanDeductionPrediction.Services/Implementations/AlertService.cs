using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class AlertService : IAlertService
    {
        private readonly IAlertRepository _alertRepository;

        public AlertService(
            IAlertRepository alertRepository)
        {
            _alertRepository = alertRepository;
        }

         
        // GET ALERTS FOR CURRENT USER
         

        public async Task<List<AlertDto>> GetAlertsAsync(
            string role,
            int userId)
        {
             
            // Validate role
             

            if (string.IsNullOrWhiteSpace(role))
            {
                throw new UnauthorizedAccessException(
                    "User role is missing.");
            }

             
            // Validate user ID
             

            if (userId <= 0)
            {
                throw new UnauthorizedAccessException(
                    "Invalid user ID.");
            }

             
            // Normalize role
             

            var normalizedRole =
                role.Trim();

             
            // Validate allowed roles
             

            if (!string.Equals(
                    normalizedRole,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    normalizedRole,
                    "LoanOfficer",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(
                    normalizedRole,
                    "Borrower",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException(
                    "Invalid user role.");
            }

             
            // Get alerts from repository
             

            return await _alertRepository
                .GetAlertsAsync(
                    normalizedRole,
                    userId);
        }

         
        // GET ALERTS FOR SPECIFIC LOAN
         

        public async Task<List<AlertDto>> GetLoanAlertsAsync(
            int loanId)
        {
             
            // Validate loan ID
             

            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan ID.");
            }

             
            // Get alerts from repository
             

            return await _alertRepository
                .GetLoanAlertsAsync(
                    loanId);
        }
    }
}