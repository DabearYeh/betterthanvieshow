using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 每日時刻表 Repository 實作
/// </summary>
public class DailyScheduleRepository : IDailyScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public DailyScheduleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<DailySchedule?> GetByDateAsync(DateTime date)
    {
        return await _context.DailySchedules
            .FirstOrDefaultAsync(ds => ds.ScheduleDate.Date == date.Date);
    }

    /// <inheritdoc />
    public async Task<DailySchedule> CreateAsync(DailySchedule schedule)
    {
        schedule.CreatedAt = DateTime.UtcNow;
        schedule.UpdatedAt = DateTime.UtcNow;
        
        _context.DailySchedules.Add(schedule);
        await _context.SaveChangesAsync();
        
        return schedule;
    }

    /// <inheritdoc />
    public async Task<DailySchedule> UpdateAsync(DailySchedule schedule)
    {
        schedule.UpdatedAt = DateTime.UtcNow;
        
        _context.DailySchedules.Update(schedule);
        await _context.SaveChangesAsync();
        
        return schedule;
    }
}
