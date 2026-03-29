using AuthService.Application.Services;

namespace AuthService.Tests;

public class FakeTokenService : ITokenService
{
    public string GenerateToken(string userId, string email, string role)
    {
        return "fake-token";
    }
}