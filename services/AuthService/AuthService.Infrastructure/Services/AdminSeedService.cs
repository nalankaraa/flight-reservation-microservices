using AuthService.Application.Repositories;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Services;

public class AdminSeedService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;

    public AdminSeedService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var section = _configuration.GetSection("AdminSeed");
        var enabled = bool.TryParse(section["Enabled"], out var configuredEnabled) ? configuredEnabled : true;

        if (!enabled)
            return;

        var email = (section["Email"] ?? "admin@system.local").Trim().ToLowerInvariant();
        var password = section["Password"] ?? "Admin123!";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var existingAdmin = await _userRepository.GetByEmailAsync(email);

        if (existingAdmin is not null)
            return;

        await _userRepository.AddAsync(new User
        {
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            Role = "Admin"
        });
    }
}
