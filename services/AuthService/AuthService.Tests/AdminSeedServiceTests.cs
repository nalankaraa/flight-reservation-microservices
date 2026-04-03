using AuthService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AuthService.Tests;

public class AdminSeedServiceTests
{
    [Fact]
    public async Task SeedAsync_Should_Create_Default_Admin_When_Admin_Does_Not_Exist()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminSeed:Enabled"] = "true",
                ["AdminSeed:Email"] = "admin@system.local",
                ["AdminSeed:Password"] = "Admin123!"
            })
            .Build();

        var service = new AdminSeedService(repository, passwordHasher, configuration);

        await service.SeedAsync();

        repository.Users.Should().ContainSingle();
        repository.Users[0].Email.Should().Be("admin@system.local");
        repository.Users[0].Role.Should().Be("Admin");
        repository.Users[0].PasswordHash.Should().Be("hashed::Admin123!");
    }

    [Fact]
    public async Task SeedAsync_Should_Not_Create_Admin_When_It_Already_Exists()
    {
        var repository = new FakeUserRepository();
        repository.Users.Add(new Domain.Entities.User
        {
            Email = "admin@system.local",
            PasswordHash = "existing",
            Role = "Admin"
        });

        var passwordHasher = new FakePasswordHasher();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminSeed:Enabled"] = "true",
                ["AdminSeed:Email"] = "admin@system.local",
                ["AdminSeed:Password"] = "Admin123!"
            })
            .Build();

        var service = new AdminSeedService(repository, passwordHasher, configuration);

        await service.SeedAsync();

        repository.Users.Should().HaveCount(1);
        repository.Users[0].PasswordHash.Should().Be("existing");
    }
}
