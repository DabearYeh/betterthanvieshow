using betterthanvieshow.Models.DTOs;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 場次服務介面
/// </summary>
public interface IShowtimeService
{
    /// <summary>
    /// 建立新場次
    /// </summary>
    /// <param name="dto">新增場次請求</param>
    /// <returns>建立的場次資訊</returns>
    Task<ShowtimeResponseDto> CreateShowtimeAsync(CreateShowtimeRequestDto dto);
}
