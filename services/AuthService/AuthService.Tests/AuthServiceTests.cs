using FluentAssertions;
using Xunit;

namespace AuthService.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_Should_Create_User_When_Email_Is_New()
    {
        var repository = new FakeUserRepository();
        var tokenService = new FakeTokenService();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService);

        var request = new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "123456",
            Role = "User"
        };

        var result = await service.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrWhiteSpace();
        repository.Users.Should().ContainSingle(u => u.Email == "test@mail.com");
    }

    [Fact]
    public async Task Register_Should_Return_Failure_When_Email_Already_Exists()
    {
        var repository = new FakeUserRepository();
        var tokenService = new FakeTokenService();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService);

        await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "123456",
            Role = "User"
        });

        var result = await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "123456",
            Role = "User"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User already exists.");
    }

    [Fact]
    public async Task Login_Should_Return_Token_When_Credentials_Are_Correct()
    {
        var repository = new FakeUserRepository();
        var tokenService = new FakeTokenService();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService);

        await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "123456",
            Role = "User"
        });

        var login = new AuthService.Application.Dtos.LoginRequestDto
        {
            Email = "test@mail.com",
            Password = "123456"
        };

        var result = await service.LoginAsync(login);

        result.Success.Should().BeTrue();
        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Should_Return_Failure_When_Password_Is_Wrong()
    {
        var repository = new FakeUserRepository();
        var tokenService = new FakeTokenService();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService);

        await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "123456",
            Role = "User"
        });

        var login = new AuthService.Application.Dtos.LoginRequestDto
        {
            Email = "test@mail.com",
            Password = "wrong"
        };

        var result = await service.LoginAsync(login);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Invalid credentials.");
    }
}