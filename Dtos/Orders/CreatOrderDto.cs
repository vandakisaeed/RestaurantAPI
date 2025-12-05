using System.ComponentModel.DataAnnotations;
using RestaurantAPI.Models;

namespace RestaurantAPI.Dtos.Orders;

public record CreateOrderDto(
[property: Required]
Guid UserId,
[property: Required]
string Description,
[property: Required]
decimal Amount,
[property: Required]
OrderType Type);