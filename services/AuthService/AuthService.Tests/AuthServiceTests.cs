using FluentAssertions;
using Xunit;

namespace AuthService.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_Should_Create_User_When_Email_Is_New()
    {
        // Arrange
        var repository = new FakeUserRepository();
        var tokenService = new FakeTokenService();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService);

        var request = new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "123456",
            Role = "User"
        };

        // Act
        var result = await service.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        repository.Users.Should().ContainSingle(u => u.Email == "test@mail.com");
    }

    [Fact]
    public async Task Register_Should_Throw_When_Email_Already_Exists()
    {
        // Arrange
        var repository = new FakeUserRepository();
        var tokenService = new FakeTokenService();
        var service = new AuthService.Application.Services.AuthService(repository, tokenService);

        await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "123456",
            Role = "User"
        });

        // Act
        var action = async () => await service.RegisterAsync(new AuthService.Application.Dtos.RegisterRequestDto
        {
            Email = "test@mail.com",
            Password = "123456",
            Role = "User"
        });

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Login_Should_Return_Token_When_Credentials_Are_Correct()
    {
        // Arrange
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

        // Act
        var result = await service.LoginAsync(login);

        // Assert
        result.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_Should_Throw_When_Password_Is_Wrong()
    {
        // Arrange
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

        // Act
        var action = async () => await service.LoginAsync(login);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }
}