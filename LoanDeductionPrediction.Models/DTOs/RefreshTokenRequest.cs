using System.ComponentModel.DataAnnotations;

namespace LoanDeductionPrediction.Models.DTOs
{
    public class RefreshTokenRequest
    {
        [Required(
            ErrorMessage =
                "Refresh token is required.")]
        [StringLength(
            1000,
            MinimumLength = 20,
            ErrorMessage =
                "Invalid refresh token.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}