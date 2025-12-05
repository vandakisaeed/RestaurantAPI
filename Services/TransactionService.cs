#region Order with storage
// using RestaurantAPI.Events;
// using RestaurantAPI.Models;

// namespace RestaurantAPI.Services;

// public class OrderService : IOrderService
// {
//     private readonly StorageService _storage;

//     public event EventHandler<OrderAddedEventArgs>? OrderAdded;

//     public OrderService(StorageService storage)
//     {
//         _storage = storage;
//     }

//     public Task<IReadOnlyList<Order>> ListAsync()
//     {
//         var all = _storage.GetAllOrders().ToList();
//         return Task.FromResult((IReadOnlyList<Order>)all);
//     }

//     public Task<IReadOnlyList<Order>> ListByUserAsync(Guid userId)
//     {
//         var list = _storage.GetAllOrders().Where(t => t.UserId == userId).ToList();
//         return Task.FromResult((IReadOnlyList<Order>)list);
//     }

//     public Task<Order?> GetAsync(Guid id)
//     {
//         var tx = _storage.GetAllOrders().FirstOrDefault(t => t.Id == id);
//         return Task.FromResult(tx);
//     }

//     public async Task<Order> CreateAsync(Guid userId, string description, decimal amount, OrderType type, DateTime? timestamp = null)
//     {
//         var now = timestamp ?? DateTime.Now;
//         var tx = new Order
//         {
//             Id = Guid.NewGuid(),
//             UserId = userId,
//             Timestamp = now,
//             Date = now.Date,
//             Type = type,
//             Description = description,
//             Amount = amount
//         };

//         _storage.SaveOrder(tx);

//         OrderAdded?.Invoke(this, new OrderAddedEventArgs(tx));

//         return tx;
//     }

//     public Task<Order?> UpdateAsync(Guid id, string? description, decimal? amount, OrderType? type)
//     {
//         var tx = _storage.GetAllOrders().FirstOrDefault(t => t.Id == id);
//         if (tx == null) return Task.FromResult<Order?>(null);

//         if (description is not null) tx.Description = description;
//         if (amount is not null) tx.Amount = amount.Value;
//         if (type is not null) tx.Type = type.Value;

//         var updated = _storage.UpdateOrder(tx);
//         return updated ? Task.FromResult<Order?>(tx) : Task.FromResult<Order?>(null);
//     }

//     public Task<bool> DeleteAsync(Guid id)
//     {
//         return Task.FromResult(_storage.DeleteOrder(id));
//     }
// }

#endregion

#region Order with db
using RestaurantAPI.Events;
using RestaurantAPI.Models;
using RestaurantAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace RestaurantAPI.Services;

// Order with db
public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;

    public event EventHandler<OrderAddedEventArgs>? OrderAdded;

    public OrderService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Order>> ListAsync()
    {
        return await _db.Orders
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Order>> ListByUserAsync(Guid userId)
    {
        return await _db.Orders
            .Where(t => t.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Order?> GetAsync(Guid id)
    {
        return await _db.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Order> CreateAsync(Guid userId, string description,
        decimal amount, OrderType type, DateTime? timestamp = null)
    {
        var now = timestamp ?? DateTime.UtcNow;

        var tx = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Timestamp = now,
            Date = now.Date,
            Type = type,
            Description = description,
            Amount = amount
        };

        _db.Orders.Add(tx);
        await _db.SaveChangesAsync();

        OrderAdded?.Invoke(this, new OrderAddedEventArgs(tx));

        return tx;
    }

    public async Task<Order?> UpdateAsync(Guid id, string? description,
        decimal? amount, OrderType? type)
    {
        var tx = await _db.Orders.FirstOrDefaultAsync(t => t.Id == id);
        if (tx == null) return null;

        if (description != null) tx.Description = description;
        if (amount != null) tx.Amount = amount.Value;
        if (type != null) tx.Type = type.Value;

        await _db.SaveChangesAsync();
        return tx;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tx = await _db.Orders.FirstOrDefaultAsync(t => t.Id == id);
        if (tx == null) return false;

        _db.Orders.Remove(tx);
        await _db.SaveChangesAsync();
        return true;
    }
}

#endregion

