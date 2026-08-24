using Microsoft.Data.SqlClient;

string connStr = "Server=.\\SQLEXPRESS;Database=LoanDeductionPredictionDb;Trusted_Connection=True;TrustServerCertificate=True;";
using var conn = new SqlConnection(connStr);
conn.Open();

string adminEmail = "admin@example.com";
string adminPass = "Admin@123";
string adminHash = BCrypt.Net.BCrypt.HashPassword(adminPass);

string officerEmail = "officer@example.com";
string officerPass = "Officer@123";
string officerHash = BCrypt.Net.BCrypt.HashPassword(officerPass);

string borrowerEmail = "borrower@example.com";
string borrowerPass = "Borrower@123";
string borrowerHash = BCrypt.Net.BCrypt.HashPassword(borrowerPass);

void EnsureUser(string fullName, string email, string hash, string role)
{
    using var checkCmd = conn.CreateCommand();
    checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
    checkCmd.Parameters.AddWithValue("@Email", email);
    int count = (int)checkCmd.ExecuteScalar()!;
    if (count == 0)
    {
        using var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive, CreatedAt)
            VALUES (@FullName, @Email, @Hash, @Role, 1, GETDATE())";
        insertCmd.Parameters.AddWithValue("@FullName", fullName);
        insertCmd.Parameters.AddWithValue("@Email", email);
        insertCmd.Parameters.AddWithValue("@Hash", hash);
        insertCmd.Parameters.AddWithValue("@Role", role);
        insertCmd.ExecuteNonQuery();
        Console.WriteLine($"Created user: {email} ({role})");
    }
    else
    {
        Console.WriteLine($"User already exists: {email}");
    }
}

EnsureUser("System Administrator", adminEmail, adminHash, "Admin");
EnsureUser("John Officer", officerEmail, officerHash, "LoanOfficer");
EnsureUser("Jane Borrower", borrowerEmail, borrowerHash, "Borrower");

Console.WriteLine("Seeding completed successfully!");