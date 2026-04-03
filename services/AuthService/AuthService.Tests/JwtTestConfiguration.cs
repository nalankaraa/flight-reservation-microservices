using Microsoft.Extensions.Configuration;

namespace AuthService.Tests;

public static class JwtTestConfiguration
{
    public static IConfiguration Create(int expiresInMinutes = 5)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ReplaceThisWithAStrongSecretKeyForDevelopment123!",
                ["Jwt:Issuer"] = "AuthService",
                ["Jwt:Audience"] = "Dispatcher",
                ["Jwt:ExpiresInMinutes"] = expiresInMinutes.ToString()
            })
            .Build();
    }
}