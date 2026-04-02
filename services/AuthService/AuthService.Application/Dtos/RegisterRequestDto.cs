using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.Dtos;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = default!;

    [Required]
    public string Role { get; set; } = default!;
}