using RestaurantAPI.Models;

namespace RestaurantAPI.Services;

public interface IOrderService
{
    event EventHandler<RestaurantAPI.Events.OrderAddedEventArgs>? OrderAdded;
    Task<IReadOnlyList<Order>> ListAsync();
    Task<IReadOnlyList<Order>> ListByUserAsync(Guid userId);
    Task<Order?> GetAsync(Guid id);
    Task<Order> CreateAsync(Guid userId, string description, decimal amount, OrderType type, DateTime? timestamp = null);
    Task<Order?> UpdateAsync(Guid id, string? description, decimal? amount, OrderType? type);
    Task<bool> DeleteAsync(Guid id);
}
