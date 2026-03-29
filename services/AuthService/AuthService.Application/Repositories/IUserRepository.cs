using AuthService.Domain.Entities;

namespace AuthService.Application.Repositories;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User?> GetByEmailAsync(string email);
}