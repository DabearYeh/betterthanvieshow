using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 座位 Repository 實作
/// </summary>
public class SeatRepository : ISeatRepository
{
    private readonly ApplicationDbContext _context;

    public SeatRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<Seat>> GetSeatsByTheaterIdAsync(int theaterId)
    {
        return await _context.Seats
            .Where(s => s.TheaterId == theaterId)
            .OrderBy(s => s.RowName)
            .ThenBy(s => s.ColumnNumber)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Seat>> GetByIdsAsync(List<int> seatIds)
    {
        return await _context.Seats
            .Where(s => seatIds.Contains(s.Id))
            .ToListAsync();
    }
}
