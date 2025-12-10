// Infrastructure/CustomHealthCheck.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RestaurantAPI.Infrastructure;

public class CustomHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomHealthCheck> _logger;

    public CustomHealthCheck(ApplicationDbContext context, ILogger<CustomHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Performing custom health check");

            // Check database connectivity by counting users
            var userCount = await _context.Users.CountAsync(cancellationToken);

            // Check if we have any data (this might indicate proper setup)
            var hasData = userCount > 0;

            // You can add more business logic checks here
            // For example: check if external APIs are reachable, check disk space, etc.

            var result = HealthCheckResult.Healthy($"Database accessible with {userCount} users. Has data: {hasData}");

            _logger.LogDebug("Custom health check passed: {Result}", result.Description);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom health check failed");
            return HealthCheckResult.Unhealthy("Custom health check failed", ex);
        }
    }
}