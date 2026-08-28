using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class LoanRequestService : ILoanRequestService
    {
        private readonly ILoanRequestRepository _loanRequestRepository;

        
        // LOAN ELIGIBILITY RULES
        

        // Salary based:
        // Maximum eligible loan = 20 × monthly salary
        private const decimal SalaryMultiplier = 20m;

        // Collateral based:
        // Maximum eligible loan = 70% of collateral value
        private const decimal CollateralPercentage = 0.70m;


        
        // CONSTRUCTOR
        

        public LoanRequestService(
            ILoanRequestRepository loanRequestRepository)
        {
            _loanRequestRepository = loanRequestRepository;
        }


        
        // CREATE LOAN REQUEST
        // BORROWER
        

        public async Task<LoanRequest> CreateRequestAsync(
            int borrowerId,
            CreateLoanRequestDto request)
        {
            
            // Validate borrower
            

            if (borrowerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid borrower ID.");
            }


            
            // Validate request
            

            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }


            
            // Validate requested amount
            

            if (request.RequestedAmount <= 0)
            {
                throw new ArgumentException(
                    "Requested amount must be greater than zero.");
            }


            
            // Validate loan type
            

            if (string.IsNullOrWhiteSpace(request.LoanType))
            {
                throw new ArgumentException(
                    "Loan type is required.");
            }


            
            // DETERMINE FINANCIAL BASIS
            

            bool hasSalary =
                request.MonthlySalary.HasValue &&
                request.MonthlySalary.Value > 0;

            bool hasCollateral =
                request.CollateralValue.HasValue &&
                request.CollateralValue.Value > 0;


            
            // Must provide one financial basis
            

            if (!hasSalary && !hasCollateral)
            {
                throw new ArgumentException(
                    "Either monthly salary or collateral value must be provided.");
            }


            
            // Cannot use both
            

            if (hasSalary && hasCollateral)
            {
                throw new ArgumentException(
                    "Provide either monthly salary or collateral value, not both.");
            }


            decimal maximumEligibleAmount;


            
            // SALARY BASED LOAN
            

            if (hasSalary)
            {
                maximumEligibleAmount =
                    request.MonthlySalary!.Value *
                    SalaryMultiplier;
            }


            
            // COLLATERAL BASED LOAN
            

            else
            {
                if (string.IsNullOrWhiteSpace(
                    request.CollateralDetails))
                {
                    throw new ArgumentException(
                        "Collateral details are required when applying using collateral.");
                }

                maximumEligibleAmount =
                    request.CollateralValue!.Value *
                    CollateralPercentage;
            }


            
            // CHECK REQUESTED AMOUNT
            

            if (request.RequestedAmount >
                maximumEligibleAmount)
            {
                throw new ArgumentException(
                    $"Requested amount exceeds the maximum eligible loan amount of {maximumEligibleAmount:0.00}.");
            }


            
            // CREATE LOAN REQUEST
            

            var loanRequest = new LoanRequest
            {
                BorrowerId = borrowerId,

                RequestedAmount =
                    request.RequestedAmount,

                // Salary is stored only for salary-based request
                MonthlySalary =
                    hasSalary
                        ? request.MonthlySalary
                        : null,

                // Collateral is stored only for
                // collateral-based request
                CollateralDetails =
                    hasCollateral
                        ? request.CollateralDetails!.Trim()
                        : null,

                CollateralValue =
                    hasCollateral
                        ? request.CollateralValue
                        : null,

                LoanType =
                    request.LoanType.Trim(),

                // Loan Officer decides these values
                InterestRate = null,

                TenureMonths = null,

                // New request starts as PENDING
                Status = "PENDING",

                RequestedAt =
                    DateTime.UtcNow,

                Remarks =
                    string.IsNullOrWhiteSpace(
                        request.Remarks)
                        ? null
                        : request.Remarks.Trim()
            };


            
            // SAVE
            

            await _loanRequestRepository
                .AddAsync(loanRequest);

            await _loanRequestRepository
                .SaveChangesAsync();


            return loanRequest;
        }


        
        // GET MY REQUESTS
        // BORROWER
        

        public async Task<List<LoanRequest>>
            GetMyRequestsAsync(
                int borrowerId)
        {
            if (borrowerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid borrower ID.");
            }

            return await _loanRequestRepository
                .GetByBorrowerIdAsync(
                    borrowerId);
        }


        
        // GET PENDING REQUESTS
        // LOAN OFFICER
        

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
            if (loanRequestId <= 0)
            {
                return null;
            }

            return await _loanRequestRepository
                .GetByIdAsync(
                    loanRequestId);
        }


        
        // APPROVE LOAN REQUEST
        // LOAN OFFICER
        //
        // Loan Officer decides:
        // - Interest rate
        // - Tenure
        

        public async Task<bool>
            ApproveRequestAsync(
                int loanRequestId,
                int loanOfficerId,
                decimal interestRate,
                int tenureMonths)
        {
            
            // Get request
            

            var request =
                await _loanRequestRepository
                    .GetByIdAsync(
                        loanRequestId);


            
            // Request not found
            

            if (request == null)
            {
                return false;
            }


            
            // Only PENDING requests can be approved
            

            if (!string.Equals(
                request.Status,
                "PENDING",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only pending loan requests can be approved.");
            }


            
            // Validate Loan Officer
            

            if (loanOfficerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan officer ID.");
            }


            
            // Validate interest rate
            

            if (interestRate <= 0 ||
                interestRate > 100)
            {
                throw new ArgumentException(
                    "Interest rate must be greater than 0 and not more than 100.");
            }


            
            // Validate tenure
            

            if (tenureMonths <= 0 ||
                tenureMonths > 360)
            {
                throw new ArgumentException(
                    "Tenure must be between 1 and 360 months.");
            }


            
            // Store approval information
            

            request.InterestRate =
                interestRate;

            request.TenureMonths =
                tenureMonths;

            request.Status =
                "APPROVED";

            request.ReviewedByLoanOfficerId =
                loanOfficerId;

            request.ReviewedAt =
                DateTime.UtcNow;


            
            // Save
            

            await _loanRequestRepository
                .SaveChangesAsync();


            return true;
        }


        
        // REJECT LOAN REQUEST
        // LOAN OFFICER
        

        public async Task<bool>
            RejectRequestAsync(
                int loanRequestId,
                int loanOfficerId,
                string? remarks)
        {
            
            // Get request
            

            var request =
                await _loanRequestRepository
                    .GetByIdAsync(
                        loanRequestId);


            
            // Request not found
            

            if (request == null)
            {
                return false;
            }


            
            // Only PENDING requests can be rejected
            

            if (!string.Equals(
                request.Status,
                "PENDING",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only pending loan requests can be rejected.");
            }


            
            // Validate Loan Officer
            

            if (loanOfficerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan officer ID.");
            }


            
            // Reject request
            

            request.Status =
                "REJECTED";

            request.ReviewedByLoanOfficerId =
                loanOfficerId;

            request.ReviewedAt =
                DateTime.UtcNow;


            
            // Save rejection remarks
            

            request.Remarks =
                string.IsNullOrWhiteSpace(remarks)
                    ? "Loan request rejected."
                    : remarks.Trim();


            
            // Save
            

            await _loanRequestRepository
                .SaveChangesAsync();


            return true;
        }
    }
}