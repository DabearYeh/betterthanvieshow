using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 儲存每日時刻表請求 DTO
/// </summary>
public class SaveDailyScheduleRequestDto
{
    /// <summary>
    /// 場次清單
    /// </summary>
    /// <example>[{"movieId": 1, "theaterId": 1, "startTime": "09:30"}, {"movieId": 2, "theaterId": 1, "startTime": "14:00"}]</example>
    [Required(ErrorMessage = "場次清單為必填")]
    public List<ShowtimeItemDto> Showtimes { get; set; } = new();
}
