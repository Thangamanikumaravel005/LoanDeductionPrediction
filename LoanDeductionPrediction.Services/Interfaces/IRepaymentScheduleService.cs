using LoanDeductionPrediction.Models.DTOs;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IRepaymentScheduleService
    {
        Task<List<RepaymentScheduleDto>> GetByLoanIdAsync(
            int loanId);

        Task<RepaymentScheduleDto?> GetByIdAsync(
            int scheduleId);

        Task<List<RepaymentScheduleDto>> GenerateScheduleAsync(
            int loanId);

        Task<RepaymentScheduleDto> RecordPaymentAsync(
            int scheduleId,
            decimal paidAmount,
            DateOnly paymentDate);
    }
}