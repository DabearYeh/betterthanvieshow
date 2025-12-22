namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 每日時刻表回應 DTO
/// </summary>
public class DailyScheduleResponseDto
{
    /// <summary>
    /// 時刻表日期
    /// </summary>
    public DateTime ScheduleDate { get; set; }

    /// <summary>
    /// 狀態：Draft（草稿）、OnSale（販售中）
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 該日期的所有場次
    /// </summary>
    public List<ShowtimeResponseDto> Showtimes { get; set; } = new();

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 最後更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
