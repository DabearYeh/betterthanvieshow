namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 簡化場次 DTO（用於分組顯示）
/// </summary>
public class ShowtimeSimpleDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 影廳 ID
    /// </summary>
    public int TheaterId { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間（HH:mm）
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（HH:mm）
    /// </summary>
    public string EndTime { get; set; } = string.Empty;
}
