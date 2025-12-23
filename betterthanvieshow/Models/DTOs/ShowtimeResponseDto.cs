namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 場次回應 DTO
/// </summary>
public class ShowtimeResponseDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 電影 ID
    /// </summary>
    public int MovieId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 電影時長（分鐘）
    /// </summary>
    public int MovieDuration { get; set; }

    /// <summary>
    /// 影廳 ID
    /// </summary>
    public int TheaterId { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型
    /// </summary>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 放映日期
    /// </summary>
    public DateTime ShowDate { get; set; }

    /// <summary>
    /// 開始時間
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（動態計算）
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// 該日期時刻表狀態
    /// </summary>
    public string ScheduleStatus { get; set; } = string.Empty;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
