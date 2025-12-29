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

    /// <summary>
    /// 開始販售時刻表
    /// </summary>
    /// <param name="date">時刻表日期</param>
    /// <returns>販售後的時刻表資訊</returns>
    Task<DailyScheduleResponseDto> PublishDailyScheduleAsync(DateTime date);

    /// <summary>
    /// 查詢每日時刻表
    /// </summary>
    /// <param name="date">時刻表日期</param>
    /// <returns>時刻表資訊及所有場次</returns>
    Task<DailyScheduleResponseDto> GetDailyScheduleAsync(DateTime date);

    /// <summary>
    /// 獲取月曆概覽
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="month">月份（1-12）</param>
    /// <returns>該月份的所有日期狀態</returns>
    Task<MonthOverviewResponseDto> GetMonthOverviewAsync(int year, int month);

    /// <summary>
    /// 複製時刻表
    /// </summary>
    /// <param name="sourceDate">來源日期</param>
    /// <param name="dto">複製請求</param>
    /// <returns>複製結果</returns>
    Task<CopyDailyScheduleResponseDto> CopyDailyScheduleAsync(DateTime sourceDate, CopyDailyScheduleRequestDto dto);

}
