string password = "Admin@123";

string hash =
    BCrypt.Net.BCrypt.HashPassword(password);

Console.WriteLine("Password:");
Console.WriteLine(password);

Console.WriteLine();
Console.WriteLine("BCrypt Hash:");
Console.WriteLine(hash);