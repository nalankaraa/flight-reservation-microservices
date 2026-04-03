using AuthService.Application.Dtos;
using AuthService.Application.Repositories;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private const string PublicRegistrationRole = "Customer";

    private readonly IUserRepository _repository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository repository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
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

        if (!string.Equals(request.Role, PublicRegistrationRole, StringComparison.OrdinalIgnoreCase))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Public registration only supports the Customer role."
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
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = PublicRegistrationRole
        };

        await _repository.AddAsync(user);

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.Role);

        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "User registered successfully.",
            User = MapToProfile(user)
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

        var user = await _repository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());

        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
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
            Message = "Login successful.",
            User = MapToProfile(user)
        };
    }

    public async Task<UserProfileDto?> GetByIdAsync(string userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        return user is null ? null : MapToProfile(user);
    }

    private static UserProfileDto MapToProfile(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
