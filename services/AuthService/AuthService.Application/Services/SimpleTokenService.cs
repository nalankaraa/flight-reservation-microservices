using System.Text;

namespace AuthService.Application.Services;

public class SimpleTokenService : ITokenService
{
    public string GenerateToken(string userId, string email, string role)
    {
        var raw = $"{userId}:{email}:{role}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }
}