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

    /// <summary>
    /// 刪除指定日期的所有場次
    /// </summary>
    /// <param name="showDate">放映日期</param>
    Task DeleteByDateAsync(DateTime showDate);

    /// <summary>
    /// 批次新增場次
    /// </summary>
    /// <param name="showtimes">場次清單</param>
    Task<List<MovieShowTime>> CreateBatchAsync(List<MovieShowTime> showtimes);

    /// <summary>
    /// 取得指定日期的所有場次（包含關聯資料）
    /// </summary>
    /// <param name="showDate">放映日期</param>
    Task<List<MovieShowTime>> GetByDateWithDetailsAsync(DateTime showDate);

    /// <summary>
    /// 取得指定電影的所有可訂票日期（時刻表狀態為 OnSale）
    /// </summary>
    /// <param name="movieId">電影 ID</param>
    /// <returns>可訂票的日期列表（已去重且排序）</returns>
    Task<List<DateTime>> GetAvailableDatesByMovieIdAsync(int movieId);
}
