using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Dispatcher.Tests;

public static class JwtTestTokenFactory
{
    private const string Key = "ReplaceThisWithAStrongSecretKeyForDevelopment123!";
    private const string Issuer = "AuthService";
    private const string Audience = "Dispatcher";

    public static string CreateToken(string role, DateTime? expiresUtc = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "user-1"),
            new(ClaimTypes.Email, "test@mail.com"),
            new(ClaimTypes.Role, role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expiresUtc ?? DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}