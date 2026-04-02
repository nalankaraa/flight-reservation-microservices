namespace AuthService.Application.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string passwordHash, string password);
}