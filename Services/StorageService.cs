using System.Text.Json;
using RestaurantAPI.Models;

namespace RestaurantAPI.Services;

public class StorageService
{
    private readonly string _dataDirectory;

    public StorageService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        if (!Directory.Exists(_dataDirectory))
            Directory.CreateDirectory(_dataDirectory);
    }

    private string FilePathForDate(DateTime date)
    {
        var fileName = date.ToString("yyyy-MM-dd") + ".json";
        return Path.Combine(_dataDirectory, fileName);
    }

    public void SaveOrder(Order tx)
    {
        // Ensure Date property set (use local date component of timestamp)
        tx.Date = tx.Timestamp.ToLocalTime().Date;

        var path = FilePathForDate(tx.Date);

        List<Order> list = new();
        if (File.Exists(path))
        {
            try
            {
                var text = File.ReadAllText(path);
                list = JsonSerializer.Deserialize<List<Order>>(text) ?? new List<Order>();
            }
            catch
            {
                // If the file is malformed, overwrite with fresh list
                list = new List<Order>();
            }
        }

        list.Add(tx);

        var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public List<Order> ReadOrders(DateTime date)
    {
        var path = FilePathForDate(date.Date);
        if (!File.Exists(path))
            return new List<Order>();

        var text = File.ReadAllText(path);
        try
        {
            return JsonSerializer.Deserialize<List<Order>>(text) ?? new List<Order>();
        }
        catch
        {
            return new List<Order>();
        }
    }

    public IEnumerable<Order> GetAllOrders()
    {
        var results = new List<Order>();
        var files = Directory.GetFiles(_dataDirectory, "*.json");
        foreach (var f in files)
        {
            try
            {
                var text = File.ReadAllText(f);
                var list = JsonSerializer.Deserialize<List<Order>>(text);
                if (list != null && list.Count > 0)
                    results.AddRange(list);
            }
            catch
            {
                // ignore malformed files
            }
        }
        return results;
    }

    public bool UpdateOrder(Order tx)
    {
        var files = Directory.GetFiles(_dataDirectory, "*.json");
        foreach (var f in files)
        {
            try
            {
                var text = File.ReadAllText(f);
                var list = JsonSerializer.Deserialize<List<Order>>(text);
                if (list == null || list.Count == 0)
                    continue;

                var idx = list.FindIndex(t => t.Id == tx.Id);
                if (idx >= 0)
                {
                    list[idx] = tx;
                    var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(f, json);
                    return true;
                }
            }
            catch
            {
                // ignore malformed files
            }
        }

        return false;
    }

    public bool DeleteOrder(Guid id)
    {
        // Scan all files in data directory to find and remove Order
        var files = Directory.GetFiles(_dataDirectory, "*.json");
        foreach (var f in files)
        {
            try
            {
                var text = File.ReadAllText(f);
                var list = JsonSerializer.Deserialize<List<Order>>(text);
                if (list == null || list.Count == 0)
                    continue;

                var origCount = list.Count;
                list.RemoveAll(t => t.Id == id);
                if (list.Count != origCount)
                {
                    // write back (if empty, write empty array)
                    var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(f, json);
                    return true;
                }
            }
            catch
            {
                // ignore malformed files
            }
        }

        return false;
    }

    public IEnumerable<Order> GetOrdersBetween(DateTime startInclusive, DateTime endInclusive)
    {
        var results = new List<Order>();
        for (var date = startInclusive.Date; date <= endInclusive.Date; date = date.AddDays(1))
        {
            results.AddRange(ReadOrders(date));
        }
        return results;
    }
}
