using AuthService.Domain.Entities;
using AuthService.Application.Repositories;

namespace AuthService.Tests;

public class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = new();

    public Task AddAsync(User user)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return Task.FromResult(Users.FirstOrDefault(u => u.Email == email));
    }
}