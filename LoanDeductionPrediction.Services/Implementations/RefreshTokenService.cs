using System.Security.Cryptography;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Repositories.Interfaces;
using LoanDeductionPrediction.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LoanDeductionPrediction.Services.Implementations
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        public async Task<RefreshToken> CreateAsync(int userId)
        {
            // Default refresh token expiration: 7 days
            var expirationDays = 7;

            var configuredExpiration =
                _configuration["Jwt:RefreshTokenExpirationDays"];

            if (int.TryParse(
                    configuredExpiration,
                    out var parsedExpiration)
                && parsedExpiration > 0)
            {
                expirationDays = parsedExpiration;
            }

            // Generate secure random refresh token
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(tokenBytes);

            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),

                // Important
                RevokedAt = null,
                IsRevoked = false
            };

            await _refreshTokenRepository.AddAsync(refreshToken);

            return refreshToken;
        }

        public async Task<RefreshToken?> GetValidTokenAsync(string token)
        {
            var refreshToken =
                await _refreshTokenRepository.GetByTokenAsync(token);

            if (refreshToken == null)
            {
                return null;
            }

            if (refreshToken.IsRevoked)
            {
                return null;
            }

            if (refreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return null;
            }

            return refreshToken;
        }

        public async Task RevokeAsync(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _refreshTokenRepository.UpdateAsync(refreshToken);
        }
    }
}