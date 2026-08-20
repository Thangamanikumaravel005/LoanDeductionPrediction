using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.UnitOfWork
{
    public interface ILoanDeductionUnitOfWork
    {
        LoanDeductionDbContext Context { get; }

        Task ExecuteInTransactionAsync(
            Func<Task> operation);
    }
}