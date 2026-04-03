using System.IdentityModel.Tokens.Jwt;
using AuthService.Application.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace AuthService.Tests;

public class TokenServiceTests
{
    [Fact]
    public void GenerateToken_Should_Use_Configured_Expiry_Minutes()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ReplaceThisWithAStrongSecretKeyForDevelopment123!",
                ["Jwt:Issuer"] = "AuthService",
                ["Jwt:Audience"] = "Dispatcher",
                ["Jwt:ExpiresInMinutes"] = "5"
            })
            .Build();

        var tokenService = new SimpleTokenService(configuration);

        var before = DateTime.UtcNow;
        var token = tokenService.GenerateToken("user-1", "test@mail.com", "Admin");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var after = DateTime.UtcNow;

        var minExpected = before.AddMinutes(5).AddSeconds(-5);
        var maxExpected = after.AddMinutes(5).AddSeconds(5);

        jwt.ValidTo.Should().BeOnOrAfter(minExpected);
        jwt.ValidTo.Should().BeOnOrBefore(maxExpected);
    }
}