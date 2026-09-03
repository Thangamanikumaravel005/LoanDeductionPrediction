using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using LoanDeductionPrediction.Models.DTOs;
using LoanDeductionPrediction.Repositories.Entities;
using LoanDeductionPrediction.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IConfiguration _configuration;

        public AuthController(
            IUserService userService,
            IRefreshTokenService refreshTokenService,
            IConfiguration configuration)
        {
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _configuration = configuration;
        }

        
        // LOGIN POST: api/Auth/login
       
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
                
            }

            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Email and password are required."
                });
            }

            var email = request.Email.Trim();

            var user = await _userService.AuthenticateAsync(
                email,
                request.Password);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new
                {
                    message = "This account is inactive."
                });
            }

            
            // Generate access token
            
            var accessToken = GenerateAccessToken(user);

            
            // Create refresh token
            
            var refreshToken =
                await _refreshTokenService.CreateAsync(user.UserId);

            
            // Access token expiration
            
            var expirationMinutes =
                GetAccessTokenExpirationMinutes();

            var response = new LoginResponse
            {
                AccessToken = accessToken,

                RefreshToken = refreshToken.Token,

                AccessTokenExpiresAt =
                    DateTime.UtcNow.AddMinutes(expirationMinutes),

                Role = user.Role
            };

            return Ok(response);
        }

        
        // REFRESH ACCESS TOKEN POST: api/Auth/refresh
        
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshTokenRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new
                {
                    message = "Refresh token is required."
                });
            }

            var refreshToken =
                await _refreshTokenService.GetValidTokenAsync(
                    request.RefreshToken);

            if (refreshToken == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid, expired, or revoked refresh token."
                });
            }

            if (refreshToken.User == null)
            {
                return Unauthorized(new
                {
                    message = "User associated with refresh token was not found."
                });
            }

            var user = refreshToken.User;

            if (!user.IsActive)
            {
                return Unauthorized(new
                {
                    message = "This account is inactive."
                });
            }

            
            // Generate new access token
        
            var accessToken = GenerateAccessToken(user);

                
            // Revoke old refresh token
            
            await _refreshTokenService.RevokeAsync(refreshToken);

                
            // Create new refresh token
            
            var newRefreshToken =
                await _refreshTokenService.CreateAsync(user.UserId);

            var expirationMinutes =
                GetAccessTokenExpirationMinutes();

            var response = new LoginResponse
            {
                AccessToken = accessToken,

                RefreshToken = newRefreshToken.Token,

                AccessTokenExpiresAt =
                    DateTime.UtcNow.AddMinutes(expirationMinutes),

                Role = user.Role
            };

            return Ok(response);
        }

        
        // GENERATE ACCESS TOKEN
       
        private string GenerateAccessToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];

            var jwtIssuer = _configuration["Jwt:Issuer"];

            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new InvalidOperationException(
                    "JWT Key is missing.");
            }

            if (string.IsNullOrWhiteSpace(jwtIssuer))
            {
                throw new InvalidOperationException(
                    "JWT Issuer is missing.");
            }

            if (string.IsNullOrWhiteSpace(jwtAudience))
            {
                throw new InvalidOperationException(
                    "JWT Audience is missing.");
            }

            var expirationMinutes =
                GetAccessTokenExpirationMinutes();

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.FullName ?? string.Empty),

                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? string.Empty),

                new Claim(
                    ClaimTypes.Role,
                    user.Role ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires:
                    DateTime.UtcNow.AddMinutes(
                        expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        
        // GET ACCESS TOKEN EXPIRATION
        
        private int GetAccessTokenExpirationMinutes()
        {
            var configuredValue =
                _configuration["Jwt:AccessTokenExpirationMinutes"];

            if (int.TryParse(
                    configuredValue,
                    out var expirationMinutes)
                && expirationMinutes > 0)
            {
                return expirationMinutes;
            }

            return 30;
        }
    }
}