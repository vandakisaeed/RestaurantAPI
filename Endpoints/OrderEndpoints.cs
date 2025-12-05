using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RestaurantAPI.Dtos.Orders;
using RestaurantAPI.Services;
using FiltersLecture.Filters;

namespace RestaurantAPI.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/Orders");

        group.MapGet("/", async (IOrderService OrderService) =>
        {
            var Orders = await OrderService.ListAsync();
            var dtos = Orders.Select(p => new OrderResponseDto(
                p.Id,
                p.UserId,
                p.Description,
                p.Amount,
                p.Type,
                p.Timestamp,
                p.Date
            ));
            return TypedResults.Ok(dtos);
        })
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest); ;

        group.MapPost("/", [Authorize] async (CreateOrderDto dto, IOrderService OrderService, HttpContext context, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var Order = await OrderService.CreateAsync(userId, dto.Description, dto.Amount, dto.Type);
            var OrderDto = new OrderResponseDto(
                Order.Id,
                Order.UserId,
                Order.Description,
                Order.Amount,
                Order.Type,
                Order.Timestamp,
                Order.Date
            );

            var location = $"{context.Request.Scheme}://{context.Request.Host}/Orders/{Order.Id}";
            return Results.Created(location, OrderDto);
        }).WithValidation<CreateOrderDto>()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest); ;

        group.MapPatch("/{id:guid}", async (Guid id, UpdateOrderDto dto, IOrderService OrderService) =>
        {
            var Order = await OrderService.UpdateAsync(id, dto.Description, dto.Amount, dto.Type);
            if (Order is null) return Results.NotFound();

            var OrderDto = new OrderResponseDto(
                Order.Id,
                Order.UserId,
                Order.Description,
                Order.Amount,
                Order.Type,
                Order.Timestamp,
                Order.Date
            );
            return TypedResults.Ok(OrderDto);
        }).Produces(200);

        group.MapGet("/{id:guid}", async (Guid id, IOrderService OrderService) =>
        {
            var Order = await OrderService.GetAsync(id);
            if (Order is null) return Results.NotFound();

            var dto = new OrderResponseDto(
                Order.Id,
                Order.UserId,
                Order.Description,
                Order.Amount,
                Order.Type,
                Order.Timestamp,
                Order.Date
            );
            return TypedResults.Ok(dto);
        }).Produces(200);

        group.MapDelete("/{id:guid}", async (Guid id, IOrderService OrderService) =>
        {
            var deleted = await OrderService.DeleteAsync(id);
            return deleted ? Results.NoContent() : Results.NotFound();
        }).Produces(200);
    }
}