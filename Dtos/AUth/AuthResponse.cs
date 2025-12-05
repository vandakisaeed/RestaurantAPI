// Dtos/Auth/AuthResponseDto.cs
namespace RestaurantAPI.Dtos.Auth;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}