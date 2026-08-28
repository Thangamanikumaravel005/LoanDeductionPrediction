namespace LoanDeductionPrediction.Models.DTOs
{
    public class CreateLoanRequestDto
    {
        public decimal RequestedAmount { get; set; }

        public decimal? MonthlySalary { get; set; }

        public string? CollateralDetails { get; set; }

        public decimal? CollateralValue { get; set; }

        public string LoanType { get; set; } = string.Empty;

        public string? Remarks { get; set; }
    }

    public class ApproveLoanRequestDto
    {
        public decimal InterestRate { get; set; }

        public int TenureMonths { get; set; }
    }
}