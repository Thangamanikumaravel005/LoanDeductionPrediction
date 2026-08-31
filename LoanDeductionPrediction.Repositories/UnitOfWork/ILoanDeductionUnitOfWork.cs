// It is used to group multiple database operations into a single unit of work, 
// ensuring that either all operations succeed or none of them are applied to the database.
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