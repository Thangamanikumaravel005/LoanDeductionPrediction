using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> CreateAsync(int userId);

        Task<RefreshToken?> GetValidTokenAsync(string token);

        Task RevokeAsync(RefreshToken refreshToken);
    }
}