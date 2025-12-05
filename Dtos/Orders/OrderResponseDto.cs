using RestaurantAPI.Models;

namespace RestaurantAPI.Dtos.Orders;

public record OrderResponseDto(
    Guid Id,
    Guid UserId,
    string Description,
    decimal Amount,
    OrderType Type,
    DateTime Timestamp,
    DateTime Date
);
