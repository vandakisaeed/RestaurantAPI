// Services/Interfaces/IMetricsService.cs
namespace RestaurantAPI.Services;

public interface IMetricsService
{
    void RecordUserCreated();
    void RecordOrderCreated();
    void RecordLoginAttempt(bool successful);
}