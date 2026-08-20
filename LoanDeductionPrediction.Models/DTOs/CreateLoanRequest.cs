using System.ComponentModel.DataAnnotations;

namespace LoanDeductionPrediction.Models.DTOs
{
    public class CreateLoanRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "BorrowerId must be greater than 0.")]
        public int BorrowerId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "LoanOfficerId must be greater than 0.")]
        public int LoanOfficerId { get; set; }

        [Range(
            typeof(decimal),
            "0.01",
            "999999999999999999",
            ErrorMessage = "PrincipalAmount must be greater than 0.")]
        public decimal PrincipalAmount { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "100",
            ErrorMessage = "InterestRate must be between 0 and 100.")]
        public decimal InterestRate { get; set; }

        [Range(
            1,
            360,
            ErrorMessage = "TenureMonths must be between 1 and 360.")]
        public int TenureMonths { get; set; }

        public DateOnly StartDate { get; set; }
    }
}