using Microsoft.AspNetCore.Identity;
namespace RestaurantAPI.Models;

public class User : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}