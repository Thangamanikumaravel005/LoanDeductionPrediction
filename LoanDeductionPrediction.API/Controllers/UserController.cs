using BCrypt.Net;
using LoanDeductionPrediction.Repositories.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanDeductionPrediction.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,LoanOfficer")]
    public class UserController : ControllerBase
    {
        private readonly LoanDeductionDbContext _context;

        public UserController(LoanDeductionDbContext context)
        {
            _context = context;
        }

        
        // GET: api/User
        // Admin / Loan Officer can view users
        

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

        
        // GET: api/User/{id}
        // Admin / Loan Officer can view one user
        

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

        
        // POST: api/User
        //
        // Admin       → can create LoanOfficer
        // LoanOfficer  → can create Borrower
        //
        // Borrower     → cannot access this controller
        

        [HttpPost]
        public async Task<IActionResult> CreateUser(
            CreateUserRequest request)
        {
            // --------------------------------------------------------
            // FULL NAME VALIDATION
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new
                {
                    message = "Full name is required."
                });
            }

            // --------------------------------------------------------
            // EMAIL VALIDATION
            // --------------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    message = "Email is required."
                });
            }

            // --------------------------------------------------------
            // PASSWORD VALIDATION
            // --------------------------------------------------------

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
                    message =
                        "Password must contain at least 8 characters."
                });
            }

            // --------------------------------------------------------
            // GET LOGGED-IN USER ROLE
            // --------------------------------------------------------

            var currentUserRole =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.Role)?.Value;

            if (string.IsNullOrWhiteSpace(currentUserRole))
            {
                return Unauthorized(new
                {
                    message =
                        "User role could not be determined."
                });
            }

            currentUserRole =
                currentUserRole.Trim();

            // --------------------------------------------------------
            // REQUESTED ROLE
            // --------------------------------------------------------

            var requestedRole =
                request.Role?.Trim();

            if (string.IsNullOrWhiteSpace(requestedRole))
            {
                return BadRequest(new
                {
                    message = "Role is required."
                });
            }

            // --------------------------------------------------------
            // ROLE-BASED USER CREATION
            //
            // Admin
            //      ↓
            // LoanOfficer
            //
            // LoanOfficer
            //      ↓
            // Borrower
            // --------------------------------------------------------

            if (string.Equals(
                    currentUserRole,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                // Admin can ONLY create Loan Officer

                if (!string.Equals(
                        requestedRole,
                        "LoanOfficer",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message =
                            "Admin can create only a LoanOfficer account."
                    });
                }
            }
            else if (string.Equals(
                         currentUserRole,
                         "LoanOfficer",
                         StringComparison.OrdinalIgnoreCase))
            {
                // Loan Officer can ONLY create Borrower

                if (!string.Equals(
                        requestedRole,
                        "Borrower",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message =
                            "Loan Officer can create only a Borrower account."
                    });
                }
            }
            else
            {
                // Any other role cannot create users

                return Forbid();
            }

            // --------------------------------------------------------
            // NORMALIZE EMAIL
            // --------------------------------------------------------

            var email = request.Email
                .Trim()
                .ToLowerInvariant();

            // --------------------------------------------------------
            // CHECK DUPLICATE EMAIL
            // --------------------------------------------------------

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == email);

            if (existingUser != null)
            {
                return Conflict(new
                {
                    message =
                        "A user with this email already exists."
                });
            }

            // --------------------------------------------------------
            // HASH PASSWORD
            // --------------------------------------------------------

            var passwordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password);

            // --------------------------------------------------------
            // DETERMINE FINAL ROLE
            // --------------------------------------------------------

            string finalRole;

            if (string.Equals(
                    requestedRole,
                    "Borrower",
                    StringComparison.OrdinalIgnoreCase))
            {
                finalRole = "Borrower";
            }
            else
            {
                finalRole = "LoanOfficer";
            }

            // --------------------------------------------------------
            // CREATE USER
            // --------------------------------------------------------

            var user = new User
            {
                FullName =
                    request.FullName.Trim(),

                Email =
                    email,

                PasswordHash =
                    passwordHash,

                Role =
                    finalRole,

                IsActive =
                    true,

                CreatedAt =
                    DateTime.UtcNow
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            // --------------------------------------------------------
            // RESPONSE
            // --------------------------------------------------------

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.UserId },
                new
                {
                    message =
                        finalRole == "Borrower"
                            ? "Borrower created successfully."
                            : "Loan Officer created successfully.",

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

        
        // PUT: api/User/{id}
        // Update existing user
        

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            UpdateUserRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // --------------------------------------------------------
            // UPDATE EMAIL
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var email = request.Email
                    .Trim()
                    .ToLowerInvariant();

                var emailExists =
                    await _context.Users
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
            // UPDATE FULL NAME
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    request.FullName))
            {
                user.FullName =
                    request.FullName.Trim();
            }

            // --------------------------------------------------------
            // UPDATE ROLE
            //
            // We don't allow changing a user to Admin.
            // We also don't allow changing role arbitrarily.
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    request.Role))
            {
                var role =
                    request.Role.Trim();

                if (!string.Equals(
                        role,
                        "LoanOfficer",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        role,
                        "Borrower",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message =
                            "Role can only be LoanOfficer or Borrower."
                    });
                }

                user.Role =
                    string.Equals(
                        role,
                        "Borrower",
                        StringComparison.OrdinalIgnoreCase)
                        ? "Borrower"
                        : "LoanOfficer";
            }

            // --------------------------------------------------------
            // UPDATE PASSWORD
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    request.Password))
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
            // UPDATE ACTIVE STATUS
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

        
        // DELETE: api/User/{id}
        // Deactivate user
        

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUser(
            int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // --------------------------------------------------------
            // PREVENT ADMIN FROM DEACTIVATING THEMSELVES
            // --------------------------------------------------------

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

            // --------------------------------------------------------
            // SOFT DELETE
            // --------------------------------------------------------

            user.IsActive = false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "User deactivated successfully."
            });
        }
    }

   
    // REQUEST MODELS
  

    public class CreateUserRequest
    {
        public string FullName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string Password { get; set; }
            = string.Empty;

        public string Role { get; set; }
            = string.Empty;
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