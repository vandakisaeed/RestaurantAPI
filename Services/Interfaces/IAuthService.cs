using RestaurantAPI.Dtos.Auth;

namespace RestaurantAPI.Services;

public interface IAuthService
{
    Task<(bool Success, IEnumerable<object> Errors)> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    Task<object?> GetCurrentUserAsync(string userId);
}