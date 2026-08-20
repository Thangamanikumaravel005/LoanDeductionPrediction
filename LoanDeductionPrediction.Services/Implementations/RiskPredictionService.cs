using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class RiskPredictionService
        : IRiskPredictionService
    {
        private readonly IRiskPredictionRepository
            _riskRepository;

        public RiskPredictionService(
            IRiskPredictionRepository riskRepository)
        {
            _riskRepository = riskRepository;
        }

         
        // GENERATE RISK PREDICTION
         

        public async Task<RiskPrediction>
            GeneratePredictionAsync(int loanId)
        {
            
            // Validate loan ID
            

            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan ID.");
            }

            
            // Get loan
            

            var loan =
                await _riskRepository
                    .GetLoanAsync(loanId);

            if (loan == null)
            {
                throw new ArgumentException(
                    "Loan not found.");
            }

            
            // Validate loan
            

            if (loan.PrincipalAmount <= 0)
            {
                throw new InvalidOperationException(
                    "Loan principal amount is invalid.");
            }

            if (loan.OutstandingAmount < 0)
            {
                throw new InvalidOperationException(
                    "Loan outstanding amount is invalid.");
            }

            
            // Get payment behavior
            

            var behaviorLogs =
                await _riskRepository
                    .GetBehaviorLogsByLoanIdAsync(
                        loanId);

            if (behaviorLogs == null ||
                behaviorLogs.Count == 0)
            {
                throw new InvalidOperationException(
                    "No payment behavior data is available for this loan.");
            }

            
            // Calculate behavior statistics
            

            int missedPayments =
                behaviorLogs.Count(p =>
                    string.Equals(
                        p.PaymentStatus,
                        "MISSED",
                        StringComparison.OrdinalIgnoreCase));

            int latePayments =
                behaviorLogs.Count(p =>
                    string.Equals(
                        p.PaymentStatus,
                        "LATE",
                        StringComparison.OrdinalIgnoreCase));

            int partialPayments =
                behaviorLogs.Count(p =>
                    string.Equals(
                        p.PaymentStatus,
                        "PARTIAL",
                        StringComparison.OrdinalIgnoreCase));

            double averageDaysLate =
                behaviorLogs.Average(p =>
                    Math.Max(
                        0,
                        p.DaysLate));

            
            // Calculate risk score
            

            decimal score = 0;

            
            // Missed payments
            

            score +=
                missedPayments * 25;

            
            // Late payments
            

            score +=
                latePayments * 10;

            
            // Partial payments
            

            score +=
                partialPayments * 15;

            
            // Average delay
            

            if (averageDaysLate > 7)
            {
                score += 15;
            }
            else if (averageDaysLate > 3)
            {
                score += 10;
            }

            
            // Outstanding balance ratio
            

            decimal outstandingRatio =
                loan.OutstandingAmount /
                loan.PrincipalAmount;

            if (outstandingRatio > 0.75m)
            {
                score += 10;
            }

            
            // Keep score between 0 and 100
            

            if (score < 0)
            {
                score = 0;
            }

            if (score > 100)
            {
                score = 100;
            }

            
            // Determine risk level
            

            string riskLevel;

            if (score < 30)
            {
                riskLevel = "LOW";
            }
            else if (score < 60)
            {
                riskLevel = "MEDIUM";
            }
            else
            {
                riskLevel = "HIGH";
            }

            
            // Generate risk reasons
            

            var reasons =
                new List<string>();

            if (missedPayments > 0)
            {
                reasons.Add(
                    $"{missedPayments} missed payment(s)");
            }

            if (latePayments > 0)
            {
                reasons.Add(
                    $"{latePayments} late payment(s)");
            }

            if (partialPayments > 0)
            {
                reasons.Add(
                    $"{partialPayments} partial payment(s)");
            }

            if (averageDaysLate > 0)
            {
                reasons.Add(
                    $"Average delay: {averageDaysLate:F2} days");
            }

            if (outstandingRatio > 0.75m)
            {
                reasons.Add(
                    "High outstanding loan balance");
            }

            if (!reasons.Any())
            {
                reasons.Add(
                    "Payment behavior is currently stable.");
            }

            string reason =
                string.Join(
                    "; ",
                    reasons);

            
            // Create prediction
            

            var prediction =
                new RiskPrediction
                {
                    BorrowerId =
                        loan.BorrowerId,

                    LoanId =
                        loan.LoanId,

                    RiskScore =
                        score,

                    RiskLevel =
                        riskLevel,

                    PredictionDate =
                        DateTime.UtcNow,

                    Reason =
                        reason
                };

            
            // Save prediction
            

            return await _riskRepository
                .AddAsync(prediction);
        }

         
        // GET BY ID
         

        public async Task<RiskPrediction?>
            GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _riskRepository
                .GetByIdAsync(id);
        }

         
        // GET BY LOAN
         

        public async Task<List<RiskPrediction>>
            GetByLoanIdAsync(int loanId)
        {
            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan ID.");
            }

            // Verify loan exists.
            var loan =
                await _riskRepository
                    .GetLoanAsync(loanId);

            if (loan == null)
            {
                throw new ArgumentException(
                    "Loan not found.");
            }

            return await _riskRepository
                .GetByLoanIdAsync(
                    loanId);
        }

         
        // GET BY BORROWER
         

        public async Task<List<RiskPrediction>>
            GetByBorrowerIdAsync(
                int borrowerId)
        {
            if (borrowerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid borrower ID.");
            }

            return await _riskRepository
                .GetByBorrowerIdAsync(
                    borrowerId);
        }
    }
}