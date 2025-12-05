using Microsoft.AspNetCore.Authorization;
using RestaurantAPI.Dtos.Orders;
using RestaurantAPI.Dtos.Users;
using RestaurantAPI.Services;
using FiltersLecture.Filters;

namespace RestaurantAPI.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users");
        // GET /users
        group.MapGet("/", async (IUserService userService) =>
        {
            var users = await userService.ListAsync();
            var userDtos = users.Select(u => new UserResponseDto(u.Id, u.Name, u.Email, u.CreatedAt));
            return TypedResults.Ok(userDtos);
        }).Produces(200);

        // Order /users
        group.MapPost("/", [Authorize] async (CreateUserDto createUserDto, IUserService userService, HttpContext context) =>
        {
            var user = await userService.CreateAsync(createUserDto.Name, createUserDto.Email);
            var userDto = new UserResponseDto(user.Id, user.Name, user.Email, user.CreatedAt);

            var location = $"{context.Request.Scheme}://{context.Request.Host}/users/{user.Id}";
            return Results.Created(location, userDto);
        }).WithValidation<CreateUserDto>()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // GET users/{id:guid}
        group.MapGet("/{id:guid}", async (Guid id, IUserService userService) =>
        {
            var user = await userService.GetAsync(id);

            if (user is null) return Results.NotFound();

            var userDto = new UserResponseDto(user.Id, user.Name, user.Email, user.CreatedAt);

            return TypedResults.Ok(userDto);
        }).Produces(200);

        // PATCH /users/{id:guid}
        group.MapPatch("/{id:guid}", async (Guid id, UpdateUserDto updateUserDto, IUserService userService) =>
        {
            var user = await userService.UpdateAsync(id, updateUserDto.Name, updateUserDto.Email);
            if (user is null)
                return Results.NotFound();

            var userDto = new UserResponseDto(user.Id, user.Name, user.Email, user.CreatedAt);
            return TypedResults.Ok(userDto);
        }).Produces(200);

        // DELETE /users/{id:guid}
        group.MapDelete("/{id:guid}", async (Guid id, IUserService userService, IOrderService OrderService) =>
        {
            var found = await userService.DeleteAsync(id);
            if (!found)
                return Results.NotFound();

            var OrdersFromUser = await OrderService.ListByUserAsync(id);

            foreach (var Order in OrdersFromUser) await OrderService.DeleteAsync(Order.Id);

            return Results.NoContent();

        }).Produces(200);

        // GET /users/{id:guid}/Orders
        group.MapGet("/{id:guid}/Orders", async (Guid id, IUserService userService, IOrderService OrderService) =>
        {
            var user = await userService.GetAsync(id);
            if (user is null)
                return Results.NotFound();

            var Orders = await OrderService.ListByUserAsync(id);
            var OrderDtos = Orders.Select(p => new OrderResponseDto(
                p.Id,
                p.UserId,
                p.Description,
                p.Amount,
                p.Type,
                p.Timestamp,
                p.Date
            ));
            return TypedResults.Ok(OrderDtos);
        }).Produces(200);
    }
}