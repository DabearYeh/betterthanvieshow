using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 驗票記錄 Repository 介面
/// </summary>
public interface ITicketValidateLogRepository
{
    /// <summary>
    /// 建立驗票記錄
    /// </summary>
    /// <param name="log">驗票記錄</param>
    /// <returns>建立後的驗票記錄</returns>
    Task<TicketValidateLog> CreateAsync(TicketValidateLog log);
}
