using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class LoanRequestService : ILoanRequestService
    {
        private readonly ILoanRequestRepository
            _loanRequestRepository;

        public LoanRequestService(
            ILoanRequestRepository loanRequestRepository)
        {
            _loanRequestRepository =
                loanRequestRepository;
        }


            
        // CREATE LOAN REQUEST
        // Borrower submits a loan request
            

        public async Task<LoanRequest>
            CreateRequestAsync(
                int borrowerId,
                CreateLoanRequestDto request)
        {
            // Validate requested amount

            if (request.RequestedAmount <= 0)
            {
                throw new ArgumentException(
                    "Requested amount must be greater than zero.");
            }


            // Validate tenure

            if (request.TenureMonths <= 0)
            {
                throw new ArgumentException(
                    "Tenure must be greater than zero.");
            }


            // Create loan request

            var loanRequest = new LoanRequest
            {
                BorrowerId = borrowerId,

                RequestedAmount =
                    request.RequestedAmount,

                TenureMonths =
                    request.TenureMonths,

                Status = "PENDING",

                RequestedAt =
                    DateTime.UtcNow,

                Remarks =
                    string.IsNullOrWhiteSpace(request.Remarks)
                        ? null
                        : request.Remarks.Trim()
            };


            // Save request

            await _loanRequestRepository
                .AddAsync(loanRequest);

            await _loanRequestRepository
                .SaveChangesAsync();


            return loanRequest;
        }


            
        // GET MY REQUESTS
        // Borrower can view their own loan requests
            

        public async Task<List<LoanRequest>>
            GetMyRequestsAsync(
                int borrowerId)
        {
            return await _loanRequestRepository
                .GetByBorrowerIdAsync(borrowerId);
        }


            
        // GET PENDING REQUESTS
        // Loan Officer can view pending requests
            

        public async Task<List<LoanRequest>>
            GetPendingRequestsAsync()
        {
            return await _loanRequestRepository
                .GetPendingAsync();
        }


            
        // GET REQUEST BY ID
            

        public async Task<LoanRequest?>
            GetByIdAsync(
                int loanRequestId)
        {
            return await _loanRequestRepository
                .GetByIdAsync(loanRequestId);
        }


            
        // APPROVE LOAN REQUEST
        // Loan Officer determines the interest rate
            

        public async Task<bool>
            ApproveRequestAsync(
                int loanRequestId,
                int loanOfficerId,
                decimal interestRate)
        {
            // Get loan request

            var request =
                await _loanRequestRepository
                    .GetByIdAsync(loanRequestId);


            // Request doesn't exist

            if (request == null)
            {
                return false;
            }


            // Only PENDING requests can be approved

            if (request.Status != "PENDING")
            {
                throw new InvalidOperationException(
                    "Only pending loan requests can be approved.");
            }


            // Validate interest rate

            if (interestRate < 0 ||
                interestRate > 100)
            {
                throw new ArgumentException(
                    "Interest rate must be between 0 and 100.");
            }


            // Loan Officer determines the interest rate

            request.InterestRate =
                interestRate;


            // Change status

            request.Status =
                "APPROVED";


            // Store the Loan Officer who approved it

            request.ReviewedByLoanOfficerId =
                loanOfficerId;


            // Store approval date

            request.ReviewedAt =
                DateTime.UtcNow;


            // Save changes

            await _loanRequestRepository
                .SaveChangesAsync();


            return true;
        }


            
        // REJECT LOAN REQUEST
        // Loan Officer rejects the request
            

        public async Task<bool>
            RejectRequestAsync(
                int loanRequestId,
                int loanOfficerId,
                string? remarks)
        {
            // Get loan request

            var request =
                await _loanRequestRepository
                    .GetByIdAsync(loanRequestId);


            // Request doesn't exist

            if (request == null)
            {
                return false;
            }


            // Only PENDING requests can be rejected

            if (request.Status != "PENDING")
            {
                throw new InvalidOperationException(
                    "Only pending loan requests can be rejected.");
            }


            // Change status

            request.Status =
                "REJECTED";


            // Store Loan Officer ID

            request.ReviewedByLoanOfficerId =
                loanOfficerId;


            // Store rejection date

            request.ReviewedAt =
                DateTime.UtcNow;


            // Store rejection remarks

            if (!string.IsNullOrWhiteSpace(remarks))
            {
                request.Remarks =
                    remarks.Trim();
            }


            // Save changes

            await _loanRequestRepository
                .SaveChangesAsync();


            return true;
        }
    }
}