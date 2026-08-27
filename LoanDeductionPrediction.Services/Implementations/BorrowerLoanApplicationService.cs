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
    public class BorrowerLoanApplicationService : IBorrowerLoanApplicationService
    {
        private readonly IBorrowerLoanApplicationRepository _applicationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoanService _loanService;
        private readonly IRepaymentScheduleService _repaymentScheduleService;
        private readonly ILoanDeductionUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BorrowerLoanApplicationService(
            IBorrowerLoanApplicationRepository applicationRepository,
            IUserRepository userRepository,
            ILoanService loanService,
            IRepaymentScheduleService repaymentScheduleService,
            ILoanDeductionUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _applicationRepository = applicationRepository;
            _userRepository = userRepository;
            _loanService = loanService;
            _repaymentScheduleService = repaymentScheduleService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // =========================================================
        // SUBMIT APPLICATION (Borrower Onboarding / Loan Application)
        // =========================================================
        public async Task<BorrowerLoanApplicationDto> SubmitApplicationAsync(
            CreateBorrowerLoanApplicationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                throw new ArgumentException("Full name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Password is required.");
            }

            if (request.Password.Length < 8)
            {
                throw new ArgumentException("Password must contain at least 8 characters.");
            }

            if (request.RequestedAmount <= 0)
            {
                throw new ArgumentException("Requested amount must be greater than zero.");
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            if (request.DateOfBirth > today)
            {
                throw new ArgumentException("Date of birth cannot be in the future.");
            }

            // Business rule: Must provide either MonthlySalary OR CollateralDetails
            bool hasSalary = request.MonthlySalary.HasValue && request.MonthlySalary.Value > 0;
            bool hasCollateral = !string.IsNullOrWhiteSpace(request.CollateralDetails);

            if (!hasSalary && !hasCollateral)
            {
                throw new ArgumentException(
                    "You must provide either Monthly Salary or Collateral Details.");
            }

            var email = request.Email.Trim().ToLower();

            // Check if there is an existing pending application with the same email
            var existingApplications = await _applicationRepository.GetByEmailAsync(email);
            if (existingApplications.Any(a => a.Status == "PENDING"))
            {
                throw new InvalidOperationException(
                    "A pending application with this email address already exists.");
            }

            // Check if email already belongs to a non-borrower account (Admin or LoanOfficer)
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null &&
                !string.Equals(existingUser.Role, "Borrower", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "This email address is already registered with a staff role.");
            }

            // Hash password using BCrypt - DO NOT store plaintext password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var application = new BorrowerLoanApplication
            {
                FullName = request.FullName.Trim(),
                DateOfBirth = request.DateOfBirth,
                Email = email,
                PasswordHash = passwordHash,
                MonthlySalary = hasSalary ? request.MonthlySalary : null,
                CollateralDetails = hasCollateral ? request.CollateralDetails!.Trim() : null,
                LoanType = request.LoanType.Trim(),
                RequestedAmount = request.RequestedAmount,
                Status = "PENDING",
                CreatedAt = DateTime.UtcNow,
                InterestRate = null,
                TenureMonths = null,
                ReviewedByLoanOfficerId = null,
                ReviewedAt = null,
                Remarks = null
            };

            var createdApplication = await _applicationRepository.AddAsync(application);

            return _mapper.Map<BorrowerLoanApplicationDto>(createdApplication);
        }

        // =========================================================
        // VIEW PENDING APPLICATIONS (Loan Officer)
        // =========================================================
        public async Task<List<BorrowerLoanApplicationDto>> GetPendingApplicationsAsync()
        {
            var applications = await _applicationRepository.GetPendingAsync();
            return _mapper.Map<List<BorrowerLoanApplicationDto>>(applications);
        }

        // =========================================================
        // VIEW ONE APPLICATION (Loan Officer)
        // =========================================================
        public async Task<BorrowerLoanApplicationDto?> GetByIdAsync(int applicationId)
        {
            if (applicationId <= 0)
            {
                return null;
            }

            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                return null;
            }

            return _mapper.Map<BorrowerLoanApplicationDto>(application);
        }

        // =========================================================
        // VIEW MY APPLICATIONS (Borrower)
        // =========================================================
        public async Task<List<BorrowerLoanApplicationDto>> GetMyApplicationsAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var applications = await _applicationRepository.GetByEmailAsync(user.Email);
            return _mapper.Map<List<BorrowerLoanApplicationDto>>(applications);
        }

        // =========================================================
        // ACCEPT APPLICATION (Loan Officer)
        // =========================================================
        public async Task<AcceptBorrowerLoanApplicationResponse> AcceptApplicationAsync(
            int applicationId,
            int loanOfficerId,
            ApproveBorrowerLoanApplicationRequest request)
        {
            if (applicationId <= 0)
            {
                throw new ArgumentException("Invalid application ID.");
            }

            if (loanOfficerId <= 0)
            {
                throw new ArgumentException("Invalid loan officer ID.");
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.InterestRate < 0 || request.InterestRate > 100)
            {
                throw new ArgumentException("Interest rate must be between 0 and 100.");
            }

            if (request.TenureMonths <= 0 || request.TenureMonths > 360)
            {
                throw new ArgumentException("Tenure must be between 1 and 360 months.");
            }

            // Verify Loan Officer
            var loanOfficer = await _userRepository.GetByIdAsync(loanOfficerId);
            if (loanOfficer == null)
            {
                throw new ArgumentException("Loan Officer not found.");
            }

            if (!loanOfficer.IsActive)
            {
                throw new ArgumentException("Loan Officer account is inactive.");
            }

            if (!string.Equals(loanOfficer.Role, "LoanOfficer", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Current user is not a Loan Officer.");
            }

            // Verify Application
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                throw new ArgumentException("Loan application not found.");
            }

            if (application.Status != "PENDING")
            {
                throw new InvalidOperationException(
                    $"Cannot accept application with status '{application.Status}'. Only PENDING applications can be accepted.");
            }

            LoanDto? createdLoanDto = null;
            User? borrowerUser = null;

            // Execute entire acceptance flow in a single transaction
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // 1. Update Application Status & Terms
                application.InterestRate = request.InterestRate;
                application.TenureMonths = request.TenureMonths;
                application.Status = "ACCEPTED";
                application.ReviewedByLoanOfficerId = loanOfficerId;
                application.ReviewedAt = DateTime.UtcNow;

                _unitOfWork.Context.BorrowerLoanApplications.Update(application);

                // 2. Create or Reuse Borrower User Account
                var normalizedEmail = application.Email.Trim().ToLower();
                borrowerUser = await _unitOfWork.Context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

                if (borrowerUser == null)
                {
                    // Create new Borrower User using the stored PasswordHash (do NOT re-hash)
                    borrowerUser = new User
                    {
                        FullName = application.FullName,
                        Email = normalizedEmail,
                        PasswordHash = application.PasswordHash,
                        Role = "Borrower",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _unitOfWork.Context.Users.Add(borrowerUser);
                    await _unitOfWork.Context.SaveChangesAsync();
                }
                else
                {
                    // Activate existing user if inactive
                    if (!borrowerUser.IsActive)
                    {
                        borrowerUser.IsActive = true;
                        _unitOfWork.Context.Users.Update(borrowerUser);
                        await _unitOfWork.Context.SaveChangesAsync();
                    }
                }

                // 3. Create LoanAccount using existing LoanService
                var createLoanRequest = new CreateLoanRequest
                {
                    BorrowerId = borrowerUser.UserId,
                    LoanOfficerId = loanOfficerId,
                    PrincipalAmount = application.RequestedAmount,
                    InterestRate = request.InterestRate,
                    TenureMonths = request.TenureMonths,
                    StartDate = DateOnly.FromDateTime(DateTime.Today)
                };

                createdLoanDto = await _loanService.CreateAsync(createLoanRequest);

                // 4. Generate Repayment Schedule using existing RepaymentScheduleService
                await _repaymentScheduleService.GenerateScheduleAsync(createdLoanDto.LoanId);
            });

            return new AcceptBorrowerLoanApplicationResponse
            {
                Message = "Borrower application accepted, account created/activated, loan account established, and repayment schedule generated successfully.",
                Application = _mapper.Map<BorrowerLoanApplicationDto>(application),
                Loan = createdLoanDto!,
                BorrowerUserId = borrowerUser!.UserId
            };
        }

        // =========================================================
        // REJECT APPLICATION (Loan Officer)
        // =========================================================
        public async Task<BorrowerLoanApplicationDto> RejectApplicationAsync(
            int applicationId,
            int loanOfficerId,
            RejectBorrowerLoanApplicationRequest request)
        {
            if (applicationId <= 0)
            {
                throw new ArgumentException("Invalid application ID.");
            }

            if (loanOfficerId <= 0)
            {
                throw new ArgumentException("Invalid loan officer ID.");
            }

            // Verify Loan Officer
            var loanOfficer = await _userRepository.GetByIdAsync(loanOfficerId);
            if (loanOfficer == null)
            {
                throw new ArgumentException("Loan Officer not found.");
            }

            if (!loanOfficer.IsActive)
            {
                throw new ArgumentException("Loan Officer account is inactive.");
            }

            if (!string.Equals(loanOfficer.Role, "LoanOfficer", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Current user is not a Loan Officer.");
            }

            // Verify Application
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                throw new ArgumentException("Loan application not found.");
            }

            if (application.Status != "PENDING")
            {
                throw new InvalidOperationException(
                    $"Cannot reject application with status '{application.Status}'. Only PENDING applications can be rejected.");
            }

            // Update status to REJECTED with remarks
            application.Status = "REJECTED";
            application.ReviewedByLoanOfficerId = loanOfficerId;
            application.ReviewedAt = DateTime.UtcNow;
            application.Remarks = string.IsNullOrWhiteSpace(request?.Remarks)
                ? null
                : request.Remarks.Trim();

            await _applicationRepository.UpdateAsync(application);

            return _mapper.Map<BorrowerLoanApplicationDto>(application);
        }
    }
}
