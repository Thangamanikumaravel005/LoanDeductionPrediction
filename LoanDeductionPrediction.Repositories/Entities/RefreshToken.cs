using System;

namespace LoanDeductionPrediction.Repositories.Entities
{
    public partial class RefreshToken
    {
        public int RefreshTokenId { get; set; }

        public int UserId { get; set; }

        public string Token { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public virtual User User { get; set; } = null!;
    }
}