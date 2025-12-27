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
                       (t.Status == "待支付" || t.Status == "未使用" || t.Status == "已使用"))
            .CountAsync();
    }

    /// <inheritdoc />
    public async Task<HashSet<int>> GetSoldSeatIdsByShowTimeAsync(int showTimeId)
    {
        var seatIds = await _context.Tickets
            .Where(t => t.ShowTimeId == showTimeId && 
                       (t.Status == "待支付" || t.Status == "未使用" || t.Status == "已使用"))
            .Select(t => t.SeatId)
            .ToListAsync();

        return new HashSet<int>(seatIds);
    }
}
