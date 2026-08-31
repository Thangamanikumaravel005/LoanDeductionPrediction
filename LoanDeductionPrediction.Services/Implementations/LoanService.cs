using AutoMapper;
using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public LoanService(
            ILoanRepository loanRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _loanRepository = loanRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        
        // GET BY ID
        

        public async Task<LoanDto?> GetByIdAsync(
            int loanId)
        {
            if (loanId <= 0)
            {
                return null;
            }

            var loan =
                await _loanRepository
                    .GetByIdAsync(loanId);

            if (loan == null)
            {
                return null;
            }

            return _mapper.Map<LoanDto>(loan);
        }

        
        // GET ALL
        

        public async Task<List<LoanDto>> GetAllAsync()
        {
            var loans =
                await _loanRepository
                    .GetAllAsync();

            return _mapper.Map<List<LoanDto>>(
                loans);
        }

        
        // GET BY BORROWER
        

        public async Task<List<LoanDto>>
            GetByBorrowerIdAsync(
                int borrowerId)
        {
            if (borrowerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid borrower ID.");
            }

            var loans =
                await _loanRepository
                    .GetByBorrowerIdAsync(
                        borrowerId);

            return _mapper.Map<List<LoanDto>>(
                loans);
        }

        
        // GET BY LOAN OFFICER
        

        public async Task<List<LoanDto>>
            GetByLoanOfficerIdAsync(
                int loanOfficerId)
        {
            if (loanOfficerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan officer ID.");
            }

            var loans =
                await _loanRepository
                    .GetByLoanOfficerIdAsync(
                        loanOfficerId);

            return _mapper.Map<List<LoanDto>>(
                loans);
        }

        
        // CREATE LOAN
        

        public async Task<LoanDto> CreateAsync(
            CreateLoanRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(
                    nameof(request));
            }

             
            // BASIC BUSINESS VALIDATION
             

            if (request.BorrowerId <= 0)
            {
                throw new ArgumentException(
                    "Borrower ID must be greater than zero.");
            }

            if (request.LoanOfficerId <= 0)
            {
                throw new ArgumentException(
                    "Loan Officer ID must be greater than zero.");
            }

            if (request.PrincipalAmount <= 0)
            {
                throw new ArgumentException(
                    "Principal amount must be greater than zero.");
            }

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

             
            // START DATE VALIDATION
             

            var today =
                DateOnly.FromDateTime(
                    DateTime.Today);

            if (request.StartDate < today)
            {
                throw new ArgumentException(
                    "Start date cannot be in the past.");
            }

             
            // BORROWER VALIDATION
             

            var borrower =
                await _userRepository
                    .GetByIdAsync(
                        request.BorrowerId);

            if (borrower == null)
            {
                throw new ArgumentException(
                    "Borrower not found.");
            }

            if (!borrower.IsActive)
            {
                throw new ArgumentException(
                    "Borrower account is inactive.");
            }

            if (!string.Equals(
                    borrower.Role,
                    "Borrower",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Selected user is not a Borrower.");
            }

             
            // LOAN OFFICER VALIDATION
             

            var loanOfficer =
                await _userRepository
                    .GetByIdAsync(
                        request.LoanOfficerId);

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
                    "Selected user is not a Loan Officer.");
            }

             
            // EMI CALCULATION
             

            var monthlyRate =
                request.InterestRate /
                12m /
                100m;

            decimal emi;

            // Zero-interest loan
            if (monthlyRate == 0)
            {
                emi =
                    request.PrincipalAmount /
                    request.TenureMonths;
            }
            else
            {
                var rate =
                    (double)monthlyRate;

                var principal =
                    (double)request.PrincipalAmount;

                var tenure =
                    request.TenureMonths;

                var power =
                    Math.Pow(
                        1 + rate,
                        tenure);

                var emiValue =
                    principal *
                    rate *
                    power /
                    (power - 1);

                emi =
                    (decimal)emiValue;
            }

            emi =
                Math.Round(
                    emi,
                    2,
                    MidpointRounding.AwayFromZero);

             
            // END DATE
             

            var endDate =
                request.StartDate.AddMonths(
                    request.TenureMonths);

             
            // CREATE LOAN ENTITY
             

            var loan =
                new LoanAccount
                {
                    BorrowerId =
                        request.BorrowerId,

                    LoanOfficerId =
                        request.LoanOfficerId,

                    PrincipalAmount =
                        request.PrincipalAmount,

                    InterestRate =
                        request.InterestRate,

                    TenureMonths =
                        request.TenureMonths,

                    Emiamount =
                        emi,

                    StartDate =
                        request.StartDate,

                    EndDate =
                        endDate,

                    OutstandingAmount =
                        request.PrincipalAmount,

                    Status =
                        "ACTIVE",

                    CreatedAt =
                        DateTime.UtcNow
                };

             
            // SAVE
             

            var createdLoan =
                await _loanRepository
                    .AddAsync(loan);

            return _mapper.Map<LoanDto>(
                createdLoan);
        }

        public async Task<bool> DeleteAsync(int loanId)
{
    if (loanId <= 0)
    {
        throw new ArgumentException(
            "Invalid loan ID.");
    }

    var loan =
        await _loanRepository.GetByIdAsync(loanId);

    if (loan == null)
    {
        return false;
    }

    if (loan.Status == "DELETED")
    {
        throw new InvalidOperationException(
            "Loan is already deleted.");
    }

    loan.Status = "DELETED";

    await _loanRepository.UpdateAsync(loan);

    return true;
}
        
        // UPDATE LOAN STATUS
        

        public async Task<bool> UpdateStatusAsync(
            int loanId,
            string status)
        {
            if (loanId <= 0)
            {
                throw new ArgumentException(
                    "Invalid loan ID.");
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException(
                    "Loan status is required.");
            }

            var loan =
                await _loanRepository
                    .GetByIdAsync(
                        loanId);

            if (loan == null)
            {
                return false;
            }

            var normalizedStatus =
                status.Trim()
                    .ToUpperInvariant();

            var validStatuses =
                new[]
                {
                    "ACTIVE",
                    "CLOSED",
                    "DEFAULTED",
                    "PENDING"
                };

            if (!validStatuses.Contains(
                    normalizedStatus))
            {
                throw new ArgumentException(
                    "Invalid loan status. " +
                    "Allowed values: ACTIVE, CLOSED, DEFAULTED, PENDING.");
            }

             
            // BUSINESS RULES FOR STATUS CHANGES
             

            if (loan.Status == "CLOSED" &&
                normalizedStatus != "CLOSED")
            {
                throw new InvalidOperationException(
                    "A closed loan cannot be reopened.");
            }

            loan.Status =
                normalizedStatus;

            await _loanRepository
                .UpdateAsync(loan);

            return true;
        }
    }
}