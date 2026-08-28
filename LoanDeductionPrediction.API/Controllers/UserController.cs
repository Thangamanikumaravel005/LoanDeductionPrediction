using BCrypt.Net;
using LoanDeductionPrediction.Repositories.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly LoanDeductionDbContext _context;

        public UserController(LoanDeductionDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: api/User
        // Admin can view all users
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .OrderBy(u => u.UserId)
                .ToListAsync();

            return Ok(users);
        }

        // ============================================================
        // GET: api/User/{id}
        // Admin can view one user
        // ============================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == id)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return Ok(user);
        }

        // ============================================================
        // POST: api/User
        // Admin can create ONLY Loan Officer
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> CreateUser(
            CreateUserRequest request)
        {
            // Full name validation
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new
                {
                    message = "Full name is required."
                });
            }

            // Email validation
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    message = "Email is required."
                });
            }

            // Password validation
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Password is required."
                });
            }

            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Password must contain at least 8 characters."
                });
            }

            // --------------------------------------------------------
            // IMPORTANT:
            // Admin can create ONLY Loan Officer.
            // Admin cannot create Borrower through this endpoint.
            // Admin cannot create another Admin through this endpoint.
            // --------------------------------------------------------

            if (!string.Equals(
                    request.Role?.Trim(),
                    "LoanOfficer",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Admin can create only a LoanOfficer account."
                });
            }

            var email = request.Email
                .Trim()
                .ToLowerInvariant();

            // Check duplicate email
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                return Conflict(new
                {
                    message =
                        "A user with this email already exists."
                });
            }

            // Hash password
            var passwordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password);

            // Create Loan Officer
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = passwordHash,
                Role = "LoanOfficer",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.UserId },
                new
                {
                    message =
                        "Loan Officer created successfully.",

                    user = new
                    {
                        user.UserId,
                        user.FullName,
                        user.Email,
                        user.Role,
                        user.IsActive,
                        user.CreatedAt
                    }
                });
        }

        // ============================================================
        // PUT: api/User/{id}
        // Admin can update a user
        // ============================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            UpdateUserRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // --------------------------------------------------------
            // Update Email
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var email = request.Email
                    .Trim()
                    .ToLowerInvariant();

                var emailExists = await _context.Users
                    .AnyAsync(u =>
                        u.Email == email &&
                        u.UserId != id);

                if (emailExists)
                {
                    return Conflict(new
                    {
                        message =
                            "Another user already uses this email."
                    });
                }

                user.Email = email;
            }

            // --------------------------------------------------------
            // Update Full Name
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName =
                    request.FullName.Trim();
            }

            // --------------------------------------------------------
            // Update Role
            //
            // Do NOT allow changing a user to Admin or Borrower
            // through this endpoint.
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var role =
                    request.Role.Trim();

                if (!string.Equals(
                        role,
                        "LoanOfficer",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message =
                            "The role can only be LoanOfficer."
                    });
                }

                user.Role = "LoanOfficer";
            }

            // --------------------------------------------------------
            // Update Password
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                if (request.Password.Length < 8)
                {
                    return BadRequest(new
                    {
                        message =
                            "Password must contain at least 8 characters."
                    });
                }

                user.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password);
            }

            // --------------------------------------------------------
            // Update Active Status
            // --------------------------------------------------------

            if (request.IsActive.HasValue)
            {
                user.IsActive =
                    request.IsActive.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "User updated successfully.",

                user = new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.IsActive,
                    user.CreatedAt
                }
            });
        }

        // ============================================================
        // DELETE: api/User/{id}
        // Admin can deactivate a user
        // ============================================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // Prevent Admin from deactivating themselves
            var currentAdminIdClaim =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier);

            if (int.TryParse(
                    currentAdminIdClaim?.Value,
                    out int currentAdminId))
            {
                if (user.UserId == currentAdminId)
                {
                    return BadRequest(new
                    {
                        message =
                            "Admin cannot deactivate their own account."
                    });
                }
            }

            user.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "User deactivated successfully."
            });
        }
    }

    // ================================================================
    // REQUEST MODELS
    // ================================================================

    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? Role { get; set; }

        public bool? IsActive { get; set; }
    }
}