using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Infrastructure.Data;

public class DbSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;

    public DbSeeder(ApplicationDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Apply migrations where possible; fall back to EnsureCreated in development.
        try
        {
            await _db.Database.MigrateAsync(ct);
        }
        catch (InvalidOperationException)
        {
            await _db.Database.EnsureCreatedAsync(ct);
        }

        const string defaultPassword = "Aa123456*";

        var alice = new User { UserName = "alice@mail", Email = "alice@mail", Name = "Alice", CreatedAt = DateTimeOffset.UtcNow };
        var bob = new User { UserName = "bob@mail", Email = "bob@mail", Name = "Bob", CreatedAt = DateTimeOffset.UtcNow };

        // Ensure the two users exist (create if missing). Do not abort seeding if other users are present.
        var a = await _userManager.FindByEmailAsync(alice.Email);
        // If UserManager didn't find the user, try a direct DbContext lookup (handles cases where normalization differs)
        if (a is null && await _db.Users.AnyAsync(ct))
        {
            a = await _db.Users.FirstOrDefaultAsync(u => u.Email == alice.Email, ct);
        }
        if (a is null)
        {
            try
            {
                var r = await _userManager.CreateAsync(alice, defaultPassword);
                if (!r.Succeeded)
                {
                    // If create failed (validation or uniqueness), attempt to recover by finding existing user.
                    a = await _userManager.FindByEmailAsync(alice.Email) ?? await _userManager.FindByNameAsync(alice.UserName);
                    if (a is null)
                        throw new InvalidOperationException($"Failed to create seed user Alice: {string.Join(',', r.Errors.Select(e => e.Description))}");
                }
                else
                {
                    a = await _userManager.FindByEmailAsync(alice.Email) ?? await _userManager.FindByNameAsync(alice.UserName);
                }
            }
            catch (DbUpdateException)
            {
                // Unique constraint race or other DB-level issue: try to load the existing user and continue.
                a = await _userManager.FindByEmailAsync(alice.Email) ?? await _userManager.FindByNameAsync(alice.UserName);
            }
        }

        var b = await _userManager.FindByEmailAsync(bob.Email);
        if (b is null && await _db.Users.AnyAsync(ct))
        {
            b = await _db.Users.FirstOrDefaultAsync(u => u.Email == bob.Email, ct);
        }
        if (b is null)
        {
            try
            {
                var r = await _userManager.CreateAsync(bob, defaultPassword);
                if (!r.Succeeded)
                {
                    b = await _userManager.FindByEmailAsync(bob.Email) ?? await _userManager.FindByNameAsync(bob.UserName);
                    if (b is null)
                        throw new InvalidOperationException($"Failed to create seed user Bob: {string.Join(',', r.Errors.Select(e => e.Description))}");
                }
                else
                {
                    b = await _userManager.FindByEmailAsync(bob.Email) ?? await _userManager.FindByNameAsync(bob.UserName);
                }
            }
            catch (DbUpdateException)
            {
                b = await _userManager.FindByEmailAsync(bob.Email) ?? await _userManager.FindByNameAsync(bob.UserName);
            }
        }

        // Seed orders only if none exist yet (so we don't duplicate on restart).
        if (!await _db.Orders.AnyAsync(ct))
        {
            var orders = new[]
            {
                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = a!.Id,
                    Description = "Pizza",
                    Amount = 14m,
                    Type = OrderType.InRestaurant,
                    Timestamp = DateTime.UtcNow,
                    Date = DateTime.UtcNow.Date
                },
                new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = b!.Id,
                    Description = "Coca",
                    Amount = 3m,
                    Type = OrderType.Takeout,
                    Timestamp = DateTime.UtcNow,
                    Date = DateTime.UtcNow.Date
                }
            };

            _db.Orders.AddRange(orders);
            await _db.SaveChangesAsync(ct);
        }
    }
}
