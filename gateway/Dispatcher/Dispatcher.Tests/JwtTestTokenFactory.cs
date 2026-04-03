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

    public static string CreateToken(string role)
    {
        return CreateToken(role, "user-1", "test@mail.com", DateTime.UtcNow.AddHours(1));
    }

    public static string CreateToken(string role, DateTime expiresUtc)
    {
        return CreateToken(role, "user-1", "test@mail.com", expiresUtc);
    }

    public static string CreateToken(string role, string userId, string email)
    {
        return CreateToken(role, userId, email, DateTime.UtcNow.AddHours(1));
    }

    public static string CreateToken(string role, string userId, string email, DateTime expiresUtc)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: expiresUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}