namespace AuthService.Application.Dtos;

public class UserProfileDto
{
	public string Id { get; set; } = default!;
	public string Email { get; set; } = default!;
	public string Role { get; set; } = default!;
	public DateTime CreatedAtUtc { get; set; }
}