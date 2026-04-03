using System.IdentityModel.Tokens.Jwt;
using AuthService.Application.Services;
using FluentAssertions;

namespace AuthService.Tests;

public class TokenServiceTests
{
    [Fact]
    public void GenerateToken_Should_Use_Configured_Expiry_Minutes()
    {
        var tokenService = new SimpleTokenService(JwtTestConfiguration.Create(5));

        var before = DateTime.UtcNow;
        var token = tokenService.GenerateToken("user-1", "test@mail.com", "Admin");
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var after = DateTime.UtcNow;

        jwt.ValidTo.Should().BeOnOrAfter(before.AddMinutes(5).AddSeconds(-5));
        jwt.ValidTo.Should().BeOnOrBefore(after.AddMinutes(5).AddSeconds(5));
    }
}