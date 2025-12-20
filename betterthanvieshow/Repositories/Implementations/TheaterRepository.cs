using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 影廳資料存取實作
/// </summary>
public class TheaterRepository : ITheaterRepository
{
    private readonly ApplicationDbContext _context;

    public TheaterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 取得所有影廳
    /// </summary>
    /// <returns>影廳實體列表</returns>
    public async Task<List<Theater>> GetAllAsync()
    {
        return await _context.Set<Theater>()
            .OrderBy(t => t.Floor)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 建立新影廳
    /// </summary>
    /// <param name="theater">影廳實體</param>
    /// <returns>建立成功的影廳實體</returns>
    public async Task<Theater> CreateAsync(Theater theater)
    {
        await _context.Theaters.AddAsync(theater);
        await _context.SaveChangesAsync();
        return theater;
    }

    /// <summary>
    /// 批次建立座位並更新影廳的 TotalSeats
    /// </summary>
    /// <param name="theaterId">影廳 ID</param>
    /// <param name="seats">座位列表</param>
    /// <param name="totalSeats">座位總數</param>
    public async Task CreateSeatsAsync(int theaterId, List<Seat> seats, int totalSeats)
    {
        // 批次新增座位
        await _context.Seats.AddRangeAsync(seats);
        
        // 更新影廳的 TotalSeats
        var theater = await _context.Theaters.FindAsync(theaterId);
        if (theater != null)
        {
            theater.TotalSeats = totalSeats;
        }
        
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 批次建立座位（不更新 TotalSeats）
    /// </summary>
    /// <param name="theaterId">影廳 ID</param>
    /// <param name="seats">座位列表</param>
    public async Task CreateSeatsOnlyAsync(int theaterId, List<Seat> seats)
    {
        // 批次新增座位
        await _context.Seats.AddRangeAsync(seats);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 根據 ID 取得影廳
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>影廳實體</returns>
    public async Task<Theater> GetByIdAsync(int id)
    {
        return await _context.Theaters.FindAsync(id) 
            ?? throw new InvalidOperationException($"找不到 ID 為 {id} 的影廳");
    }

    /// <summary>
    /// 根據 ID 取得影廳及其所有座位
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>影廳實體（包含座位），若不存在回傳 null</returns>
    public async Task<Theater?> GetByIdWithSeatsAsync(int id)
    {
        return await _context.Theaters
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// 檢查影廳是否存在
    /// </summary>
    /// <param name="id">影廳 ID</param>
    /// <returns>存在回傳 true，否則回傳 false</returns>
    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Theaters.AnyAsync(t => t.Id == id);
    }

    /// <summary>
    /// 刪除影廳及其所有座位
    /// </summary>
    /// <param name="id">影廳 ID</param>
    public async Task DeleteAsync(int id)
    {
        // 使用 Transaction 確保資料一致性
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 先刪除所有關聯的座位
            var seats = await _context.Seats
                .Where(s => s.TheaterId == id)
                .ToListAsync();
            
            if (seats.Any())
            {
                _context.Seats.RemoveRange(seats);
            }

            // 再刪除影廳
            var theater = await _context.Theaters.FindAsync(id);
            if (theater != null)
            {
                _context.Theaters.Remove(theater);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
