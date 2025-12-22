using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 場次 Repository 實作
/// </summary>
public class ShowtimeRepository : IShowtimeRepository
{
    private readonly ApplicationDbContext _context;

    public ShowtimeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<MovieShowTime> CreateAsync(MovieShowTime showtime)
    {
        showtime.CreatedAt = DateTime.UtcNow;
        
        _context.MovieShowTimes.Add(showtime);
        await _context.SaveChangesAsync();
        
        return showtime;
    }

    /// <inheritdoc />
    public async Task<MovieShowTime?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.MovieShowTimes
            .Include(st => st.Movie)
            .Include(st => st.Theater)
            .FirstOrDefaultAsync(st => st.Id == id);
    }

    /// <inheritdoc />
    public async Task<bool> HasTimeConflictAsync(
        int theaterId, 
        DateTime showDate, 
        TimeSpan startTime, 
        int durationMinutes,
        int? excludeShowtimeId = null)
    {
        var endTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));

        var query = _context.MovieShowTimes
            .Include(st => st.Movie)
            .Where(st => st.TheaterId == theaterId && st.ShowDate.Date == showDate.Date);

        if (excludeShowtimeId.HasValue)
        {
            query = query.Where(st => st.Id != excludeShowtimeId.Value);
        }

        var existingShowtimes = await query.ToListAsync();

        foreach (var existing in existingShowtimes)
        {
            var existingEndTime = existing.StartTime.Add(TimeSpan.FromMinutes(existing.Movie.Duration));
            
            // 檢查時間重疊：新場次的 [startTime, endTime) 與現有場次的 [existing.StartTime, existingEndTime) 是否重疊
            // 重疊條件：startTime < existingEndTime && endTime > existing.StartTime
            if (startTime < existingEndTime && endTime > existing.StartTime)
            {
                return true;
            }
        }

        return false;
    }
}
