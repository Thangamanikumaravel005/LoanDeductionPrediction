using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Repositories.UnitOfWork;
using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class BorrowerLoanApplicationService
        : IBorrowerLoanApplicationService
    {
        private readonly IBorrowerLoanApplicationRepository
            _applicationRepository;

        private readonly IUserRepository
            _userRepository;

        private readonly ILoanService
            _loanService;

        private readonly IRepaymentScheduleService
            _repaymentScheduleService;

        private readonly ILoanDeductionUnitOfWork
            _unitOfWork;

        private readonly IMapper
            _mapper;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public BorrowerLoanApplicationService(
            IBorrowerLoanApplicationRepository applicationRepository,
            IUserRepository userRepository,
            ILoanService loanService,
            IRepaymentScheduleService repaymentScheduleService,
            ILoanDeductionUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _applicationRepository =
                applicationRepository;

            _userRepository =
                userRepository;

            _loanService =
                loanService;

            _repaymentScheduleService =
                repaymentScheduleService;

            _unitOfWork =
                unitOfWork;

            _mapper =
                mapper;
        }


        // =========================================================
        // SUBMIT LOAN APPLICATION
        // Borrower
        // =========================================================

        public async Task<BorrowerLoanApplicationDto>
            SubmitApplicationAsync(
                CreateBorrowerLoanApplicationRequest request,
                int borrowerId)
        {
            // -----------------------------------------------------
            // Basic validation
            // -----------------------------------------------------

            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

            if (borrowerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid borrower ID.");
            }

            if (request.RequestedAmount <= 0)
            {
                throw new ArgumentException(
                    "Requested amount must be greater than zero.");
            }

            if (request.DateOfBirth >
                DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ArgumentException(
                    "Date of birth cannot be in the future.");
            }


            // -----------------------------------------------------
            // Get existing borrower
            // -----------------------------------------------------

            var borrower =
                await _userRepository
                    .GetByIdAsync(borrowerId);

            if (borrower == null)
            {
                throw new InvalidOperationException(
                    "Borrower account not found.");
            }

            if (!borrower.IsActive)
            {
                throw new InvalidOperationException(
                    "Borrower account is inactive.");
            }

            if (!string.Equals(
                borrower.Role,
                "Borrower",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Only a Borrower can submit a loan application.");
            }


            // -----------------------------------------------------
            // Salary / collateral validation
            // -----------------------------------------------------

            bool hasSalary =
                request.MonthlySalary.HasValue &&
                request.MonthlySalary.Value > 0;

            bool hasCollateral =
                !string.IsNullOrWhiteSpace(
                    request.CollateralDetails);

            if (!hasSalary && !hasCollateral)
            {
                throw new ArgumentException(
                    "You must provide either Monthly Salary or Collateral Details.");
            }


            // -----------------------------------------------------
            // Check pending application
            // -----------------------------------------------------

            var existingApplications =
                await _applicationRepository
                    .GetByEmailAsync(borrower.Email);

            if (existingApplications.Any(
                a => a.Status == "PENDING"))
            {
                throw new InvalidOperationException(
                    "You already have a pending loan application.");
            }


            // -----------------------------------------------------
            // Create application
            // -----------------------------------------------------

            var application =
                new BorrowerLoanApplication
                {
                    // IMPORTANT:
                    // Link application to existing borrower
                    BorrowerId = borrowerId,

                    // Keep these as application information
                    // because they already exist in your entity
                    FullName =
                        borrower.FullName,

                    DateOfBirth =
                        request.DateOfBirth,

                    Email =
                        borrower.Email
                            .Trim()
                            .ToLowerInvariant(),

                    // Password is NOT created here.
                    // Borrower already has an account.
                    //
                    // If PasswordHash is still required by your
                    // existing database/entity, we temporarily
                    // copy the existing user's hash.
                    PasswordHash =
                        borrower.PasswordHash,

                    MonthlySalary =
                        hasSalary
                            ? request.MonthlySalary
                            : null,

                    CollateralDetails =
                        hasCollateral
                            ? request.CollateralDetails!.Trim()
                            : null,

                    LoanType =
                        request.LoanType.Trim(),

                    RequestedAmount =
                        request.RequestedAmount,

                    Status =
                        "PENDING",

                    CreatedAt =
                        DateTime.UtcNow,

                    InterestRate =
                        null,

                    TenureMonths =
                        null,

                    ReviewedByLoanOfficerId =
                        null,

                    ReviewedAt =
                        null,

                    Remarks =
                        null
                };


            // -----------------------------------------------------
            // Save application
            // -----------------------------------------------------

            var createdApplication =
                await _applicationRepository
                    .AddAsync(application);


            // -----------------------------------------------------
            // Return DTO
            // -----------------------------------------------------

            return _mapper.Map<
                BorrowerLoanApplicationDto>(
                    createdApplication);
        }


        // =========================================================
        // GET PENDING APPLICATIONS
        // Loan Officer
        // =========================================================

        public async Task<
            List<BorrowerLoanApplicationDto>>
            GetPendingApplicationsAsync()
        {
            var applications =
                await _applicationRepository
                    .GetPendingAsync();

            return _mapper.Map<
                List<BorrowerLoanApplicationDto>>(
                applications);
        }


        // =========================================================
        // GET APPLICATION BY ID
        // =========================================================

        public async Task<
            BorrowerLoanApplicationDto?>
            GetByIdAsync(
                int applicationId)
        {
            if (applicationId <= 0)
            {
                return null;
            }

            var application =
                await _applicationRepository
                    .GetByIdAsync(
                        applicationId);

            if (application == null)
            {
                return null;
            }

            return _mapper.Map<
                BorrowerLoanApplicationDto>(
                application);
        }


        // =========================================================
        // GET MY APPLICATIONS
        // Borrower
        // =========================================================

        public async Task<
            List<BorrowerLoanApplicationDto>>
            GetMyApplicationsAsync(
                int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException(
                    "Invalid user ID.");
            }

            var user =
                await _userRepository
                    .GetByIdAsync(userId);

            if (user == null)
            {
                throw new ArgumentException(
                    "User not found.");
            }

            if (!string.Equals(
                user.Role,
                "Borrower",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "User is not a Borrower.");
            }

            var applications =
                await _applicationRepository
                    .GetByEmailAsync(
                        user.Email);

            return _mapper.Map<
                List<BorrowerLoanApplicationDto>>(
                applications);
        }


        // =========================================================
        // ACCEPT APPLICATION
        // Loan Officer
        // =========================================================

        public async Task<
            AcceptBorrowerLoanApplicationResponse>
            AcceptApplicationAsync(
                int applicationId,
                int loanOfficerId,
                ApproveBorrowerLoanApplicationRequest request)
        {
            if (applicationId <= 0)
            {
                throw new ArgumentException(
                    "Invalid application ID.");
            }

            if (loanOfficerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan officer ID.");
            }

            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }


            // -----------------------------------------------------
            // Validate loan terms
            // -----------------------------------------------------

            if (request.InterestRate < 0 ||
                request.InterestRate > 100)
            {
                throw new ArgumentException(
                    "Interest rate must be between 0 and 100.");
            }

            if (request.TenureMonths <= 0 ||
                request.TenureMonths > 360)
            {
                throw new ArgumentException(
                    "Tenure must be between 1 and 360 months.");
            }


            // -----------------------------------------------------
            // Verify Loan Officer
            // -----------------------------------------------------

            var loanOfficer =
                await _userRepository
                    .GetByIdAsync(
                        loanOfficerId);

            if (loanOfficer == null)
            {
                throw new ArgumentException(
                    "Loan Officer not found.");
            }

            if (!loanOfficer.IsActive)
            {
                throw new ArgumentException(
                    "Loan Officer account is inactive.");
            }

            if (!string.Equals(
                loanOfficer.Role,
                "LoanOfficer",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Current user is not a Loan Officer.");
            }


            // -----------------------------------------------------
            // Get application
            // -----------------------------------------------------

            var application =
                await _applicationRepository
                    .GetByIdAsync(
                        applicationId);

            if (application == null)
            {
                throw new ArgumentException(
                    "Loan application not found.");
            }


            // -----------------------------------------------------
            // Check application status
            // -----------------------------------------------------

            if (application.Status != "PENDING")
            {
                throw new InvalidOperationException(
                    $"Cannot accept application with status " +
                    $"'{application.Status}'. " +
                    $"Only PENDING applications can be accepted.");
            }


            // -----------------------------------------------------
            // Get existing borrower
            // -----------------------------------------------------

            var borrower =
                await _userRepository
                    .GetByIdAsync(
                        application.BorrowerId);

            if (borrower == null)
            {
                throw new InvalidOperationException(
                    "Borrower account associated with this application was not found.");
            }

            if (!borrower.IsActive)
            {
                throw new InvalidOperationException(
                    "Borrower account is inactive.");
            }

            if (!string.Equals(
                borrower.Role,
                "Borrower",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The application is not associated with a valid Borrower.");
            }


            LoanDto? createdLoanDto = null;


            // -----------------------------------------------------
            // Transaction
            // -----------------------------------------------------

            await _unitOfWork
                .ExecuteInTransactionAsync(
                async () =>
                {
                    // -------------------------------------------------
                    // 1. Update application
                    // -------------------------------------------------

                    application.InterestRate =
                        request.InterestRate;

                    application.TenureMonths =
                        request.TenureMonths;

                    application.Status =
                        "ACCEPTED";

                    application.ReviewedByLoanOfficerId =
                        loanOfficerId;

                    application.ReviewedAt =
                        DateTime.UtcNow;

                    _unitOfWork.Context
                        .BorrowerLoanApplications
                        .Update(application);


                    // -------------------------------------------------
                    // 2. Create LoanAccount
                    // -------------------------------------------------

                    var createLoanRequest =
                        new CreateLoanRequest
                        {
                            BorrowerId =
                                borrower.UserId,

                            LoanOfficerId =
                                loanOfficerId,

                            PrincipalAmount =
                                application.RequestedAmount,

                            InterestRate =
                                request.InterestRate,

                            TenureMonths =
                                request.TenureMonths,

                            StartDate =
                                DateOnly.FromDateTime(
                                    DateTime.Today)
                        };


                    createdLoanDto =
                        await _loanService
                            .CreateAsync(
                                createLoanRequest);


                    // -------------------------------------------------
                    // 3. Generate repayment schedule
                    // -------------------------------------------------

                    await _repaymentScheduleService
                        .GenerateScheduleAsync(
                            createdLoanDto.LoanId);
                });


            // -----------------------------------------------------
            // Return response
            // -----------------------------------------------------

            return new AcceptBorrowerLoanApplicationResponse
            {
                Message =
                    "Borrower application accepted, loan account established, and repayment schedule generated successfully.",

                Application =
                    _mapper.Map<
                        BorrowerLoanApplicationDto>(
                        application),

                Loan =
                    createdLoanDto!,

                BorrowerUserId =
                    borrower.UserId
            };
        }


        // =========================================================
        // REJECT APPLICATION
        // Loan Officer
        // =========================================================

        public async Task<
            BorrowerLoanApplicationDto>
            RejectApplicationAsync(
                int applicationId,
                int loanOfficerId,
                RejectBorrowerLoanApplicationRequest request)
        {
            if (applicationId <= 0)
            {
                throw new ArgumentException(
                    "Invalid application ID.");
            }

            if (loanOfficerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan officer ID.");
            }


            // -----------------------------------------------------
            // Verify Loan Officer
            // -----------------------------------------------------

            var loanOfficer =
                await _userRepository
                    .GetByIdAsync(
                        loanOfficerId);

            if (loanOfficer == null)
            {
                throw new ArgumentException(
                    "Loan Officer not found.");
            }

            if (!loanOfficer.IsActive)
            {
                throw new ArgumentException(
                    "Loan Officer account is inactive.");
            }

            if (!string.Equals(
                loanOfficer.Role,
                "LoanOfficer",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Current user is not a Loan Officer.");
            }


            // -----------------------------------------------------
            // Get application
            // -----------------------------------------------------

            var application =
                await _applicationRepository
                    .GetByIdAsync(
                        applicationId);

            if (application == null)
            {
                throw new ArgumentException(
                    "Loan application not found.");
            }


            // -----------------------------------------------------
            // Check status
            // -----------------------------------------------------

            if (application.Status != "PENDING")
            {
                throw new InvalidOperationException(
                    $"Cannot reject application with status " +
                    $"'{application.Status}'. " +
                    $"Only PENDING applications can be rejected.");
            }


            // -----------------------------------------------------
            // Reject
            // -----------------------------------------------------

            application.Status =
                "REJECTED";

            application.ReviewedByLoanOfficerId =
                loanOfficerId;

            application.ReviewedAt =
                DateTime.UtcNow;

            application.Remarks =
                string.IsNullOrWhiteSpace(
                    request?.Remarks)
                    ? null
                    : request.Remarks.Trim();


            await _applicationRepository
                .UpdateAsync(
                    application);


            return _mapper.Map<
                BorrowerLoanApplicationDto>(
                application);
        }
    }
}