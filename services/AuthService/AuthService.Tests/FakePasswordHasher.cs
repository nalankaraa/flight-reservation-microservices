using AuthService.Application.Services;

namespace AuthService.Tests;

public class FakePasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return $"hashed::{password}";
    }

    public bool VerifyPassword(string passwordHash, string password)
    {
        return passwordHash == HashPassword(password);
    }
}