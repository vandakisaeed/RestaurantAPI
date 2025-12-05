using RestaurantAPI.Events;
using RestaurantAPI.Services;

namespace RestaurantAPI.Services;

public class LoggerService
{
    private readonly string _logFilePath;

    public LoggerService(string logsDirectory, IOrderService txService)
    {
        if (!Directory.Exists(logsDirectory))
            Directory.CreateDirectory(logsDirectory);

        _logFilePath = Path.Combine(logsDirectory, "Orders.log");

        txService.OrderAdded += OnOrderAdded;
    }

    private void OnOrderAdded(object? sender, OrderAddedEventArgs e)
    {
        var line = $"{DateTime.UtcNow:u} | {e.Order.Type} | {e.Order.Amount:C} | {e.Order.Description} | Id={e.Order.Id}";
        try
        {
            File.AppendAllText(_logFilePath, line + Environment.NewLine);
        }
        catch
        {
            // Logging failures should not crash the app; swallow silently or consider writing to console.
            Console.WriteLine("Warning: failed to write to log file.");
        }
    }
}
