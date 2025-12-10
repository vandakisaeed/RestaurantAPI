// Services/MetricsService.cs
using System.Diagnostics.Metrics;

namespace RestaurantAPI.Services;

public class MetricsService : IMetricsService
{
    private readonly Meter _meter;
    private readonly Counter<int> _userCreatedCounter;
    private readonly Counter<int> _OrderCreatedCounter;
    private readonly Counter<int> _loginAttemptCounter;

    public MetricsService()
    {
        _meter = new Meter("RestaurantAPI", "1.0.0");

        _userCreatedCounter = _meter.CreateCounter<int>(
            "restaureant_api_users_created_total",
            "Count",
            "Total number of users created");

        _OrderCreatedCounter = _meter.CreateCounter<int>(
            "restaurant_api_Orders_created_total",
            "Count",
            "Total number of Orders created");

        _loginAttemptCounter = _meter.CreateCounter<int>(
            "restaurant_api_login_attempts_total",
            "Count",
            "Total number of login attempts");
    }

    public void RecordUserCreated() => _userCreatedCounter.Add(1);

    public void RecordOrderCreated() => _OrderCreatedCounter.Add(1);

    public void RecordLoginAttempt(bool successful) =>
        _loginAttemptCounter.Add(1, new KeyValuePair<string, object?>("successful", successful));
}