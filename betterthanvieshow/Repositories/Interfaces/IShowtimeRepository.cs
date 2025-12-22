using betterthanvieshow.Models.Entities;

namespace betterthanvieshow.Repositories.Interfaces;

/// <summary>
/// 場次 Repository 介面
/// </summary>
public interface IShowtimeRepository
{
    /// <summary>
    /// 建立新的場次
    /// </summary>
    Task<MovieShowTime> CreateAsync(MovieShowTime showtime);

    /// <summary>
    /// 根據 ID 取得場次（包含關聯資料）
    /// </summary>
    Task<MovieShowTime?> GetByIdWithDetailsAsync(int id);

    /// <summary>
    /// 檢查指定影廳、日期、時間是否有時間衝突
    /// </summary>
    /// <param name="theaterId">影廳 ID</param>
    /// <param name="showDate">放映日期</param>
    /// <param name="startTime">開始時間</param>
    /// <param name="durationMinutes">電影時長（分鐘）</param>
    /// <param name="excludeShowtimeId">排除的場次 ID（用於編輯時排除自己）</param>
    Task<bool> HasTimeConflictAsync(int theaterId, DateTime showDate, TimeSpan startTime, int durationMinutes, int? excludeShowtimeId = null);
}
