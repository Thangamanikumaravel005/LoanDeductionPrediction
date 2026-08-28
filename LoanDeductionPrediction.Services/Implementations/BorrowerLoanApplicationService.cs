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
using Microsoft.EntityFrameworkCore;

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


        
        // LOAN ELIGIBILITY RULES
        

        // Salary based loan:
        // Maximum loan = 20 times monthly salary
        private const decimal SalaryMultiplier = 20m;

        // Collateral based loan:
        // Maximum loan = 70% of collateral value
        private const decimal CollateralPercentage = 0.70m;


        
        // CONSTRUCTOR
        

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


        
        // SUBMIT APPLICATION
        // BORROWER
        

        public async Task<BorrowerLoanApplicationDto>
            SubmitApplicationAsync(
                CreateBorrowerLoanApplicationRequest request)
        {
            
            // BASIC VALIDATION
            

            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }


            if (string.IsNullOrWhiteSpace(
                request.FullName))
            {
                throw new ArgumentException(
                    "Full name is required.");
            }


            if (string.IsNullOrWhiteSpace(
                request.Email))
            {
                throw new ArgumentException(
                    "Email is required.");
            }


            if (string.IsNullOrWhiteSpace(
                request.Password))
            {
                throw new ArgumentException(
                    "Password is required.");
            }


            if (request.Password.Length < 8)
            {
                throw new ArgumentException(
                    "Password must contain at least 8 characters.");
            }


            if (request.RequestedAmount <= 0)
            {
                throw new ArgumentException(
                    "Requested amount must be greater than zero.");
            }


            if (string.IsNullOrWhiteSpace(
                request.LoanType))
            {
                throw new ArgumentException(
                    "Loan type is required.");
            }


            
            // DATE OF BIRTH VALIDATION
            

            var today =
                DateOnly.FromDateTime(
                    DateTime.Today);

            if (request.DateOfBirth > today)
            {
                throw new ArgumentException(
                    "Date of birth cannot be in the future.");
            }


            
            // FINANCIAL VALIDATION
            

            bool hasSalary =
                request.MonthlySalary.HasValue &&
                request.MonthlySalary.Value > 0;


            bool hasCollateral =
                request.CollateralValue.HasValue &&
                request.CollateralValue.Value > 0;


            
            // MUST PROVIDE SALARY OR COLLATERAL
            

            if (!hasSalary && !hasCollateral)
            {
                throw new ArgumentException(
                    "Either monthly salary or collateral value must be provided.");
            }


            
            // DO NOT ALLOW BOTH
            

            if (hasSalary && hasCollateral)
            {
                throw new ArgumentException(
                    "Provide either monthly salary or collateral value, not both.");
            }


            decimal maximumEligibleAmount;


            
            // SALARY BASED VALIDATION
            

            if (hasSalary)
            {
                maximumEligibleAmount =
                    request.MonthlySalary!.Value *
                    SalaryMultiplier;
            }


            
            // COLLATERAL BASED VALIDATION
            

            else
            {
                if (string.IsNullOrWhiteSpace(
                    request.CollateralDetails))
                {
                    throw new ArgumentException(
                        "Collateral details are required when using collateral.");
                }


                maximumEligibleAmount =
                    request.CollateralValue!.Value *
                    CollateralPercentage;
            }


            
            // REQUESTED AMOUNT VALIDATION
            

            if (request.RequestedAmount >
                maximumEligibleAmount)
            {
                throw new ArgumentException(
                    $"Requested amount exceeds the maximum eligible loan amount of {maximumEligibleAmount:0.00}.");
            }


            
            // EMAIL VALIDATION
            

            var email =
                request.Email
                    .Trim()
                    .ToLowerInvariant();


            
            // CHECK EXISTING PENDING APPLICATION
            

            var existingApplications =
                await _applicationRepository
                    .GetByEmailAsync(email);


            if (existingApplications.Any(
                a => string.Equals(
                    a.Status,
                    "PENDING",
                    StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "A pending application with this email address already exists.");
            }


            
            // CHECK EXISTING USER
            

            var existingUser =
                await _userRepository
                    .GetByEmailAsync(email);


            // Admin / LoanOfficer cannot use same email
            if (existingUser != null &&
                !string.Equals(
                    existingUser.Role,
                    "Borrower",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This email address is already registered with a staff role.");
            }


            
            // HASH PASSWORD
            

            var passwordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password);


            
            // CREATE APPLICATION
            

            var application =
                new BorrowerLoanApplication
                {
                    FullName =
                        request.FullName.Trim(),

                    DateOfBirth =
                        request.DateOfBirth,

                    Email =
                        email,

                    PasswordHash =
                        passwordHash,

                    // Salary is stored only when
                    // salary-based application is used
                    MonthlySalary =
                        hasSalary
                            ? request.MonthlySalary
                            : null,

                    // Collateral details are stored only
                    // when collateral-based application is used
                    CollateralDetails =
                        hasCollateral
                            ? request.CollateralDetails!.Trim()
                            : null,

                    // NEW:
                    // Store collateral value
                    CollateralValue =
                        hasCollateral
                            ? request.CollateralValue
                            : null,

                    LoanType =
                        request.LoanType.Trim(),

                    RequestedAmount =
                        request.RequestedAmount,

                    // Application waits for Loan Officer
                    Status =
                        "PENDING",

                    CreatedAt =
                        DateTime.UtcNow,

                    // Officer decides these later
                    InterestRate =
                        null,

                    TenureMonths =
                        null,

                    ReviewedByLoanOfficerId =
                        null,

                    ReviewedAt =
                        null,

                    Remarks =
                        string.IsNullOrWhiteSpace(
                            request.Remarks)
                            ? null
                            : request.Remarks.Trim()
                };


            
            // SAVE APPLICATION
            

            var createdApplication =
                await _applicationRepository
                    .AddAsync(application);


            return _mapper.Map<BorrowerLoanApplicationDto>(
                createdApplication);
        }


        
        // VIEW PENDING APPLICATIONS
        // LOAN OFFICER
        

        public async Task<List<BorrowerLoanApplicationDto>>
            GetPendingApplicationsAsync()
        {
            var applications =
                await _applicationRepository
                    .GetPendingAsync();

            return _mapper.Map<
                List<BorrowerLoanApplicationDto>>(
                    applications);
        }


        
        // VIEW ONE APPLICATION
        

        public async Task<BorrowerLoanApplicationDto?>
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


        
        // VIEW MY APPLICATIONS
        // BORROWER
        

        public async Task<List<BorrowerLoanApplicationDto>>
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
                    .GetByIdAsync(
                        userId);


            if (user == null)
            {
                throw new ArgumentException(
                    "User not found.");
            }


            var applications =
                await _applicationRepository
                    .GetByEmailAsync(
                        user.Email);


            return _mapper.Map<
                List<BorrowerLoanApplicationDto>>(
                    applications);
        }


        
        // ACCEPT APPLICATION
        // LOAN OFFICER
        

        public async Task<AcceptBorrowerLoanApplicationResponse>
            AcceptApplicationAsync(
                int applicationId,
                int loanOfficerId,
                ApproveBorrowerLoanApplicationRequest request)
        {
            
            // BASIC VALIDATION
            

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


            
            // INTEREST RATE
            

            if (request.InterestRate < 0 ||
                request.InterestRate > 100)
            {
                throw new ArgumentException(
                    "Interest rate must be between 0 and 100.");
            }


            
            // TENURE
            

            if (request.TenureMonths <= 0 ||
                request.TenureMonths > 360)
            {
                throw new ArgumentException(
                    "Tenure must be between 1 and 360 months.");
            }


            
            // VERIFY LOAN OFFICER
            

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


            
            // GET APPLICATION
            

            var application =
                await _applicationRepository
                    .GetByIdAsync(
                        applicationId);


            if (application == null)
            {
                throw new ArgumentException(
                    "Loan application not found.");
            }


            
            // ONLY PENDING APPLICATIONS
            

            if (!string.Equals(
                application.Status,
                "PENDING",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Cannot accept application with status '{application.Status}'. Only PENDING applications can be accepted.");
            }


            LoanDto? createdLoanDto =
                null;

            User? borrowerUser =
                null;


            
            // ACCEPTANCE TRANSACTION
            

            await _unitOfWork
                .ExecuteInTransactionAsync(
                    async () =>
                    {
                        // -------------------------------------------------
                        // 1. UPDATE APPLICATION
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


                        _unitOfWork
                            .Context
                            .BorrowerLoanApplications
                            .Update(application);


                        // -------------------------------------------------
                        // 2. FIND EXISTING USER
                        // -------------------------------------------------

                        var normalizedEmail =
                            application.Email
                                .Trim()
                                .ToLower();


                        borrowerUser =
                            await _unitOfWork
                                .Context
                                .Users
                                .FirstOrDefaultAsync(
                                    u =>
                                        u.Email.ToLower()
                                        == normalizedEmail);


                        // =================================================
                        // CREATE BORROWER USER
                        // =================================================

                        if (borrowerUser == null)
                        {
                            borrowerUser =
                                new User
                                {
                                    FullName =
                                        application.FullName,

                                    Email =
                                        normalizedEmail,

                                    // IMPORTANT:
                                    // Use existing hash.
                                    // Do NOT hash again.
                                    PasswordHash =
                                        application.PasswordHash,

                                    Role =
                                        "Borrower",

                                    IsActive =
                                        true,

                                    CreatedAt =
                                        DateTime.UtcNow
                                };


                            _unitOfWork
                                .Context
                                .Users
                                .Add(borrowerUser);


                            await _unitOfWork
                                .Context
                                .SaveChangesAsync();
                        }


                        // =================================================
                        // EXISTING USER
                        // =================================================

                        else
                        {
                            // If the user already exists but is inactive,
                            // activate the account.
                            if (!borrowerUser.IsActive)
                            {
                                borrowerUser.IsActive =
                                    true;

                                _unitOfWork
                                    .Context
                                    .Users
                                    .Update(
                                        borrowerUser);


                                await _unitOfWork
                                    .Context
                                    .SaveChangesAsync();
                            }


                            // Safety check:
                            // Only Borrower accounts should reach
                            // this stage.
                            if (!string.Equals(
                                borrowerUser.Role,
                                "Borrower",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                throw new InvalidOperationException(
                                    "The email belongs to a non-borrower account.");
                            }
                        }


                        // =================================================
                        // 3. CREATE LOAN
                        // =================================================

                        var createLoanRequest =
                            new CreateLoanRequest
                            {
                                BorrowerId =
                                    borrowerUser.UserId,

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


                        // =================================================
                        // 4. GENERATE REPAYMENT SCHEDULE
                        // =================================================

                        await _repaymentScheduleService
                            .GenerateScheduleAsync(
                                createdLoanDto.LoanId);
                    });


            
            // RETURN ACCEPTANCE RESPONSE
            

            return new AcceptBorrowerLoanApplicationResponse
            {
                Message =
                    "Borrower application accepted, account created/activated, loan account established, and repayment schedule generated successfully.",

                Application =
                    _mapper.Map<
                        BorrowerLoanApplicationDto>(
                            application),

                Loan =
                    createdLoanDto!,

                BorrowerUserId =
                    borrowerUser!.UserId
            };
        }


        
        // REJECT APPLICATION
        // LOAN OFFICER
        

        public async Task<BorrowerLoanApplicationDto>
            RejectApplicationAsync(
                int applicationId,
                int loanOfficerId,
                RejectBorrowerLoanApplicationRequest request)
        {
            
            // VALIDATE APPLICATION ID
            

            if (applicationId <= 0)
            {
                throw new ArgumentException(
                    "Invalid application ID.");
            }


            
            // VALIDATE LOAN OFFICER ID
            

            if (loanOfficerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan officer ID.");
            }


            
            // VERIFY LOAN OFFICER
            

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


            
            // GET APPLICATION
            

            var application =
                await _applicationRepository
                    .GetByIdAsync(
                        applicationId);


            if (application == null)
            {
                throw new ArgumentException(
                    "Loan application not found.");
            }


            
            // ONLY PENDING APPLICATIONS
            

            if (!string.Equals(
                application.Status,
                "PENDING",
                StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Cannot reject application with status '{application.Status}'. Only PENDING applications can be rejected.");
            }


            
            // UPDATE APPLICATION
            

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


            
            // SAVE
            

            await _applicationRepository
                .UpdateAsync(
                    application);


            return _mapper.Map<
                BorrowerLoanApplicationDto>(
                    application);
        }
    }
}