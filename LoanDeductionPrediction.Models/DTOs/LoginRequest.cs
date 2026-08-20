using System.ComponentModel.DataAnnotations;

namespace LoanDeductionPrediction.Models.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [StringLength(
            255,
            ErrorMessage = "Email cannot exceed 255 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage =
                "Password must be between 6 and 100 characters.")]
        public string Password { get; set; } = string.Empty;
    }
}