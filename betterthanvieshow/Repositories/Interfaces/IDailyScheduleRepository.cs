using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 每日時刻表 Repository 介面
/// </summary>
public interface IDailyScheduleRepository
{
    /// <summary>
    /// 根據日期取得時刻表
    /// </summary>
    /// <param name="date">時刻表日期</param>
    Task<DailySchedule?> GetByDateAsync(DateTime date);

    /// <summary>
    /// 建立新的每日時刻表
    /// </summary>
    Task<DailySchedule> CreateAsync(DailySchedule schedule);

    /// <summary>
    /// 更新每日時刻表
    /// </summary>
    Task<DailySchedule> UpdateAsync(DailySchedule schedule);

    /// <summary>
    /// 獲取指定月份的所有時刻表
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="month">月份（1-12）</param>
    /// <returns>該月份的所有時刻表記錄</returns>
    Task<List<DailySchedule>> GetByMonthAsync(int year, int month);

}
