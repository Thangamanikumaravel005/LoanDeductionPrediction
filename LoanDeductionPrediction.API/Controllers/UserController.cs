using BCrypt.Net;
using LoanDeductionPrediction.Repositories.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly LoanDeductionDbContext _context;

        public UserController(LoanDeductionDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET: api/User
        // Only Admin can view all users
        // =========================================================

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
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


        // =========================================================
        // GET: api/User/1
        // Only Admin can view one user
        // =========================================================

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
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


        // =========================================================
        // POST: api/User/loan-officer
        // Only Admin can create a Loan Officer
        // =========================================================

        [HttpPost("loan-officer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateLoanOfficer(
            CreateUserRequest request)
        {
            // Validate full name
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new
                {
                    message = "Full name is required."
                });
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    message = "Email is required."
                });
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Password is required."
                });
            }

            // Validate password length
            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Password must contain at least 8 characters."
                });
            }

            // Clean email
            var email = request.Email.Trim().ToLower();

            // Check duplicate email
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                return Conflict(new
                {
                    message = "A user with this email already exists."
                });
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password);

            // Create Loan Officer
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = passwordHash,

                // Role is assigned by the server
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
                    message = "Loan Officer created successfully.",
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


        // =========================================================
        // POST: api/User/borrower
        // Only Loan Officer can create a Borrower
        // =========================================================

        [HttpPost("borrower")]
        [Authorize(Roles = "LoanOfficer")]
        public async Task<IActionResult> CreateBorrower(
            CreateUserRequest request)
        {
            // Validate full name
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new
                {
                    message = "Full name is required."
                });
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    message = "Email is required."
                });
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    message = "Password is required."
                });
            }

            // Validate password length
            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message = "Password must contain at least 8 characters."
                });
            }

            // Clean email
            var email = request.Email.Trim().ToLower();

            // Check duplicate email
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                return Conflict(new
                {
                    message = "A user with this email already exists."
                });
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password);

            // Create Borrower
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = passwordHash,

                // Role is assigned by the server
                Role = "Borrower",

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
                    message = "Borrower created successfully.",
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


        // =========================================================
        // PUT: api/User/1
        // Only Admin can update a user
        // =========================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
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

            // Update email
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var email = request.Email.Trim().ToLower();

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

            // Update full name
            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName.Trim();
            }

            // Update password
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

            // Update active status
            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User updated successfully.",
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


        // =========================================================
        // DELETE: api/User/1
        // Only Admin can deactivate a user
        // =========================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
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

            // Soft delete
            user.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User deactivated successfully."
            });
        }
    }


     
    // REQUEST MODELS
     

    public class CreateUserRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }


    public class UpdateUserRequest
    {
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public bool? IsActive { get; set; }
    }
}