using System.ComponentModel.DataAnnotations;

namespace LoanDeductionPrediction.Models.DTOs
{
    public class RecordPaymentRequest
    {
        [Range(
            typeof(decimal),
            "0.01",
            "999999999999999999",
            ErrorMessage =
                "PaidAmount must be greater than 0.")]
        public decimal PaidAmount { get; set; }

        public DateOnly PaymentDate { get; set; }
    }
}