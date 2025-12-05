// Api/AuthEndpoints.cs
using System.Security.Claims;
using RestaurantAPI.Dtos.Auth;
using RestaurantAPI.Services;
using Microsoft.AspNetCore.Authorization;
using FiltersLecture.Filters;

namespace RestaurantAPI.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        // Register new users
        group.MapPost("/register", async (RegisterRequestDto req, IAuthService authService) =>
        {
            var (success, errors) = await authService.RegisterAsync(req);
            return success ? Results.Ok() : Results.BadRequest(errors);
        })
        .WithValidation<RegisterRequestDto>()
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // Login existing users
        group.MapPost("/login", async (LoginRequestDto req, IAuthService authService) =>
        {
            var result = await authService.LoginAsync(req);
            return result is not null ? Results.Ok(result) : Results.Unauthorized();
        })
        .WithValidation<LoginRequestDto>()
        .Produces<AuthResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        // Get current user
        group.MapGet("/me", [Authorize] async (ClaimsPrincipal user, IAuthService authService) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Results.Unauthorized();

            var currentUser = await authService.GetCurrentUserAsync(userId);
            return currentUser is not null ? Results.Ok(currentUser) : Results.NotFound();
        });
    }
}