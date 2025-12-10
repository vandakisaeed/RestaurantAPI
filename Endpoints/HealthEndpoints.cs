// RestaurantAPI/Api/HealthEndpoints.cs
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RestaurantAPI.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/health").WithTags("Health");

        group.MapGet("/", async (HealthCheckService healthCheckService) =>
        {
            var report = await healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds,
                results = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.TotalMilliseconds,
                    description = entry.Value.Description,
                    data = entry.Value.Data,
                    exception = entry.Value.Exception?.Message
                })
            };

            return report.Status == HealthStatus.Healthy
                ? Results.Ok(response)
                : Results.Problem("Health check failed", statusCode: 503);
        })
        .WithDescription("Returns the health status of the application and all its dependencies");

        group.MapGet("/ready", async (HealthCheckService healthCheckService) =>
        {
            var report = await healthCheckService.CheckHealthAsync(check => check.Tags.Contains("ready"));

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds,
                results = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    duration = entry.Value.Duration.TotalMilliseconds,
                    description = entry.Value.Description
                })
            };

            return report.Status == HealthStatus.Healthy
                ? Results.Ok(response)
                : Results.Problem("Readiness check failed", statusCode: 503);
        })
        .WithDescription("Returns whether the application is ready to receive traffic");

        group.MapGet("/live", () =>
        {
            var response = new
            {
                status = "Healthy",
                message = "Application is running",
                timestamp = DateTimeOffset.UtcNow
            };

            return Results.Ok(response);
        })
        .WithDescription("Returns whether the application is alive and responding");
    }
}