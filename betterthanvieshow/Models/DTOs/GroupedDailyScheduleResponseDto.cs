namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 分組時刻表回應 DTO（用於側邊欄顯示）
/// </summary>
public class GroupedDailyScheduleResponseDto
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
    /// 按電影分組的場次列表
    /// </summary>
    public List<MovieShowtimeGroupDto> MovieShowtimes { get; set; } = new();
}
