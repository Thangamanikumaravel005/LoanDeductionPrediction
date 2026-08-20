using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> AuthenticateAsync(
            string email,
            string password);

        Task<UserDto?> GetByIdAsync(
            int userId);

        Task<List<UserDto>> GetAllAsync();
    }
}