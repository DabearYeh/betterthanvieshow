using betterthanvieshow.Models.DTOs;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 場次 Service 介面
/// </summary>
public interface IShowtimeService
{
    /// <summary>
    /// 取得場次的座位配置
    /// </summary>
    /// <param name="showtimeId">場次 ID</param>
    /// <returns>座位配置回應 DTO</returns>
    Task<ShowtimeSeatsResponseDto?> GetShowtimeSeatsAsync(int showtimeId);
}
