namespace LoanDeductionPrediction.Models.DTOs
{
    public class CreateLoanRequestDto
    {
        public decimal RequestedAmount { get; set; }

        public int TenureMonths { get; set; }

        public string? Remarks { get; set; }
    }


    public class ApproveLoanRequestDto
    {
        public decimal InterestRate { get; set; }
    }
}