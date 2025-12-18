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
}
