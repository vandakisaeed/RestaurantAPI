using RestaurantAPI.Models;

namespace RestaurantAPI.Dtos.Orders;

public record UpdateOrderDto(string? Description, decimal? Amount, OrderType? Type);