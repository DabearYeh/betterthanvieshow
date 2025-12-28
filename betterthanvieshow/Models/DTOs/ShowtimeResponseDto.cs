namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 場次回應 DTO
/// </summary>
public class ShowtimeResponseDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    /// <example>100</example>
    public int Id { get; set; }

    /// <summary>
    /// 電影 ID
    /// </summary>
    /// <example>1</example>
    public int MovieId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    /// <example>阿凡達</example>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 電影時長（分鐘）
    /// </summary>
    /// <example>192</example>
    public int MovieDuration { get; set; }

    /// <summary>
    /// 影廳 ID
    /// </summary>
    public int TheaterId { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    /// <example>1廳</example>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型
    /// </summary>
    /// <example>一般數位</example>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 放映日期
    /// </summary>
    public DateTime ShowDate { get; set; }

    /// <summary>
    /// 開始時間
    /// </summary>
    /// <example>10:00</example>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（動態計算）
    /// </summary>
    /// <example>13:12</example>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// 該日期時刻表狀態
    /// </summary>
    /// <example>OnSale</example>
    public string ScheduleStatus { get; set; } = string.Empty;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
