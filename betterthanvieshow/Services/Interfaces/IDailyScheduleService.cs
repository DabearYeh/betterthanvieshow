using betterthanvieshow.Models.DTOs;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 每日時刻表服務介面
/// </summary>
public interface IDailyScheduleService
{
    /// <summary>
    /// 儲存每日時刻表
    /// </summary>
    /// <param name="date">時刻表日期</param>
    /// <param name="dto">時刻表資料</param>
    /// <returns>儲存後的時刻表資訊</returns>
    Task<DailyScheduleResponseDto> SaveDailyScheduleAsync(DateTime date, SaveDailyScheduleRequestDto dto);
}
