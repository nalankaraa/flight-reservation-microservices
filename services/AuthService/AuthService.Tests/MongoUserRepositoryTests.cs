using AuthService.Domain.Entities;
using AuthService.Infrastructure.Repositories;
using FluentAssertions;

namespace AuthService.Tests;

public class MongoUserRepositoryTests : IClassFixture<MongoUserRepositoryTestFixture>
{
    private readonly MongoUserRepositoryTestFixture _fixture;

    public MongoUserRepositoryTests(MongoUserRepositoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_Then_GetByEmailAsync_Should_Return_User_With_Normalized_Email()
    {
        var repository = new MongoUserRepository(_fixture.CreateDatabase());
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = "customer@test.local",
            PasswordHash = "hashed-password",
            Role = "Customer",
            CreatedAtUtc = DateTime.UtcNow
        };

        await repository.AddAsync(user);

        var result = await repository.GetByEmailAsync(" Customer@Test.Local ");

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Stored_User()
    {
        var repository = new MongoUserRepository(_fixture.CreateDatabase());
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = $"admin-{Guid.NewGuid():N}@test.local",
            PasswordHash = "hashed-admin-password",
            Role = "Admin",
            CreatedAtUtc = DateTime.UtcNow
        };

        await repository.AddAsync(user);

        var result = await repository.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
        result.Role.Should().Be("Admin");
    }
}