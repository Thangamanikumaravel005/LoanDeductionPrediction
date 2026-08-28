namespace LoanDeductionPrediction.Models.DTOs
{
    public class CreateLoanRequest
    {
        public int BorrowerId { get; set; }

        public int LoanOfficerId { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal InterestRate { get; set; }

        public int TenureMonths { get; set; }

        public DateOnly StartDate { get; set; }
    }
}