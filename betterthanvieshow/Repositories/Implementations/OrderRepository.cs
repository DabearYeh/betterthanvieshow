using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 訂單資料存取實作
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Order> CreateAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
        return order;
    }

    /// <inheritdoc/>
    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.ShowTime)
                .ThenInclude(s => s.Movie)
            .Include(o => o.ShowTime)
                .ThenInclude(s => s.Theater)
            .Include(o => o.Tickets)
                .ThenInclude(t => t.Seat)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Include(o => o.User)
            .Include(o => o.ShowTime)
                .ThenInclude(s => s.Movie)
            .Include(o => o.ShowTime)
                .ThenInclude(s => s.Theater)
            .Include(o => o.Tickets)
                .ThenInclude(t => t.Seat)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    /// <inheritdoc/>
    public async Task<bool> OrderNumberExistsAsync(string orderNumber)
    {
        return await _context.Orders
            .AnyAsync(o => o.OrderNumber == orderNumber);
    }

    /// <inheritdoc/>
    public async Task<Order> UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }
}
