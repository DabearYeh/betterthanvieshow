using betterthanvieshow.Data;
using betterthanvieshow.Models.Entities;
using betterthanvieshow.Repositories.Interfaces;

namespace betterthanvieshow.Repositories.Implementations;

/// <summary>
/// 驗票記錄 Repository 實作
/// </summary>
public class TicketValidateLogRepository : ITicketValidateLogRepository
{
    private readonly ApplicationDbContext _context;

    public TicketValidateLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<TicketValidateLog> CreateAsync(TicketValidateLog log)
    {
        await _context.TicketValidateLogs.AddAsync(log);
        await _context.SaveChangesAsync();
        return log;
    }
}
