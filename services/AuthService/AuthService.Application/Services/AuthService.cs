using AuthService.Application.Dtos;
using AuthService.Application.Repositories;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository repository, ITokenService tokenService)
    {
        _repository = repository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existingUser = await _repository.GetByEmailAsync(request.Email);

        if (existingUser != null)
            throw new Exception("User already exists");

        var user = new User
        {
            Email = request.Email,
            Password = request.Password,
            Role = request.Role
        };

        await _repository.AddAsync(user);

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role);

        return new AuthResponseDto
        {
            Token = token
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);

        if (user == null || user.Password != request.Password)
            throw new Exception("Invalid credentials");

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role);

        return new AuthResponseDto
        {
            Token = token
        };
    }
}