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
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Role))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Email, password and role are required."
            };
        }

        var existingUser = await _repository.GetByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User already exists."
            };
        }

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
            Success = true,
            Token = token,
            Message = "User registered successfully."
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Email and password are required."
            };
        }

        var user = await _repository.GetByEmailAsync(request.Email);

        if (user == null || user.Password != request.Password)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid credentials."
            };
        }

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role);

        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "Login successful."
        };
    }
}