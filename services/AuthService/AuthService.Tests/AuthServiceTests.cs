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
        var passwordHasher = new FakePasswordHasher();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService, passwordHasher);

        var request = new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "12345678",
            Role = "Customer"
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
        var passwordHasher = new FakePasswordHasher();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService, passwordHasher);

        await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "12345678",
            Role = "Customer"
        });

        var result = await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "12345678",
            Role = "Customer"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("User already exists.");
    }

    [Fact]
    public async Task Login_Should_Return_Token_When_Credentials_Are_Correct()
    {
        var repository = new FakeUserRepository();
        var tokenService = new FakeTokenService();
        var passwordHasher = new FakePasswordHasher();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService, passwordHasher);

        await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "12345678",
            Role = "Customer"
        });

        var login = new AuthService.Application.Dtos.LoginRequestDto
        {
            Email = "test@mail.com",
            Password = "12345678"
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
        var passwordHasher = new FakePasswordHasher();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService, passwordHasher);

        await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "12345678",
            Role = "Customer"
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

    [Fact]
    public async Task Register_Should_Return_Failure_When_Role_Is_Invalid()
    {
        var repository = new FakeUserRepository();
        var tokenService = new FakeTokenService();
        var passwordHasher = new FakePasswordHasher();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService, passwordHasher);

        var result = await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "12345678",
            Role = "User"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Public registration only supports the Customer role.");
    }

    [Fact]
    public async Task Register_Should_Return_Failure_When_Public_Request_Tries_To_Create_Admin()
    {
        var repository = new FakeUserRepository();
        var tokenService = new FakeTokenService();
        var passwordHasher = new FakePasswordHasher();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService, passwordHasher);

        var result = await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "admin@mail.com",
            Password = "12345678",
            Role = "Admin"
        });

        result.Success.Should().BeFalse();
        result.Message.Should().Be("Public registration only supports the Customer role.");
        repository.Users.Should().BeEmpty();
    }
}
