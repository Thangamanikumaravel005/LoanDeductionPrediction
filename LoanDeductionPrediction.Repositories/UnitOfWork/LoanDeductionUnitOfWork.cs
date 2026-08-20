using LoanDeductionPrediction.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.Repositories.UnitOfWork
{
    public class LoanDeductionUnitOfWork
        : ILoanDeductionUnitOfWork
    {
        private readonly LoanDeductionDbContext _context;

        public LoanDeductionDbContext Context =>
            _context;

        public LoanDeductionUnitOfWork(
            LoanDeductionDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteInTransactionAsync(
            Func<Task> operation)
        {
            var strategy =
                _context.Database
                    .CreateExecutionStrategy();

            await strategy.ExecuteAsync(
                async () =>
                {
                    await using var transaction =
                        await _context.Database
                            .BeginTransactionAsync();

                    try
                    {
                        // Execute all database changes
                        await operation();

                        // Save all tracked changes
                        await _context.SaveChangesAsync();

                        // Commit transaction
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        // Rollback if anything fails
                        await transaction.RollbackAsync();

                        throw;
                    }
                });
        }
    }
}