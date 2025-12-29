using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 票券 Repository 實作
/// </summary>
public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> GetSoldTicketCountByShowTimeAsync(int showTimeId)
    {
        // 只計算有效票券：待支付、未使用、已使用
        // 已過期的票券不計入
        return await _context.Tickets
            .Where(t => t.ShowTimeId == showTimeId && 
                       (t.Status == "Pending" || t.Status == "Unused" || t.Status == "Used"))
            .CountAsync();
    }

    /// <inheritdoc />
    public async Task<HashSet<int>> GetSoldSeatIdsByShowTimeAsync(int showTimeId)
    {
        var seatIds = await _context.Tickets
            .Where(t => t.ShowTimeId == showTimeId && 
                       (t.Status == "Pending" || t.Status == "Unused" || t.Status == "Used"))
            .Select(t => t.SeatId)
            .ToListAsync();

        return new HashSet<int>(seatIds);
    }

    /// <inheritdoc />
    public async Task<List<Ticket>> CreateBatchAsync(List<Ticket> tickets)
    {
        await _context.Tickets.AddRangeAsync(tickets);
        await _context.SaveChangesAsync();
        return tickets;
    }

    /// <inheritdoc />
    public async Task<bool> IsSeatOccupiedAsync(int showTimeId, int seatId)
    {
        return await _context.Tickets
            .AnyAsync(t => t.ShowTimeId == showTimeId && 
                          t.SeatId == seatId &&
                          (t.Status == "Pending" || t.Status == "Unused" || t.Status == "Used"));
    }

    /// <inheritdoc />
    public async Task<bool> TicketNumberExistsAsync(string ticketNumber)
    {
        return await _context.Tickets
            .AnyAsync(t => t.TicketNumber == ticketNumber);
    }

    /// <inheritdoc />
    public async Task<List<Ticket>> GetByOrderIdAsync(int orderId)
    {
        return await _context.Tickets
            .Where(t => t.OrderId == orderId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Ticket> UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    /// <inheritdoc />
    public async Task<Seat?> GetSeatByTicketIdAsync(int ticketId)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Seat)
            .FirstOrDefaultAsync(t => t.Id == ticketId);
        
        return ticket?.Seat;
    }
}
