namespace LoanDeductionPrediction.Models.DTOs
{
    public class RepaymentScheduleDto
    {
        public int ScheduleId { get; set; }

        public int LoanId { get; set; }

        public int InstallmentNumber { get; set; }

        public DateOnly DueDate { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal InterestAmount { get; set; }

        public decimal EmiAmount { get; set; }

        public decimal PaidAmount { get; set; }

        public DateOnly? PaidDate { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}