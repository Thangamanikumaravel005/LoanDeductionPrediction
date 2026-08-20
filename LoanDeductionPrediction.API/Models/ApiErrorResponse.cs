namespace LoanDeductionPrediction.API.Models
{
    public class ApiErrorResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public string ErrorCode { get; set; } = string.Empty;

        public string TraceId { get; set; } = string.Empty;

        public object? Errors { get; set; }
    }
}