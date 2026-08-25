using LoanDeductionPrediction.Repositories.Entities;

namespace LoanDeductionPrediction.Repositories.Seed
{
    public static class DatabaseSeeder
    {
        public static void Seed(LoanDeductionDbContext context)
        {
            // Check whether an Admin already exists
            if (context.Users.Any(u => u.Role == "Admin"))
            {
                return;
            }

            // Create default Admin
            var passwordHash =
                BCrypt.Net.BCrypt.HashPassword("Admin@123");

            var admin = new User
            {
                FullName = "System Administrator",
                Email = "systemadmin@loan.com",
                PasswordHash = passwordHash,
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            context.Users.Add(admin);

            context.SaveChanges();
        }
    }
}