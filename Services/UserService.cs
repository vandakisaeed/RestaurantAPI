using RestaurantAPI.Models;
using RestaurantAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace RestaurantAPI.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<UserService> _logger;
    private readonly IMetricsService _metrics;
    public UserService(ApplicationDbContext db, ILogger<UserService> logger, IMetricsService metrics)
    {
        _db = db;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<User?> GetAsync(Guid id)
    {
        _logger.LogInformation("Retrieving user with ID: {UserId}", id);
        return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IReadOnlyList<User>> ListAsync()
    {
        return await _db.Users.AsNoTracking().ToListAsync();
    }

    public async Task<User> CreateAsync(string name, string email)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        _metrics.RecordUserCreated();
        return user;
    }

    public async Task<User?> UpdateAsync(Guid id, string? name, string? email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return null;
        if (name is not null) user.Name = name;
        if (email is not null) user.Email = email;
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return false;
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }
}
