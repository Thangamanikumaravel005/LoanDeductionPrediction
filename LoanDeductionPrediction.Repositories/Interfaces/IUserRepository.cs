using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByIdAsync(int userId);

        Task<List<User>> GetAllAsync();

        Task<bool> EmailExistsAsync(string email);

        Task<User> AddAsync(User user);

        Task UpdateAsync(User user);
    }
}