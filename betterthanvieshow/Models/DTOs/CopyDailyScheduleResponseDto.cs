namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 複製時刻表回應 DTO
/// </summary>
public class CopyDailyScheduleResponseDto
{
    /// <summary>
    /// 來源日期
    /// </summary>
    public DateTime SourceDate { get; set; }

    /// <summary>
    /// 目標日期
    /// </summary>
    public DateTime TargetDate { get; set; }

    /// <summary>
    /// 成功複製的場次數量
    /// </summary>
    public int CopiedCount { get; set; }

    /// <summary>
    /// 被略過的場次數量（因電影檔期問題）
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// 提示訊息（如：部分場次因電影檔期已過期未複製）
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 目標時刻表資訊
    /// </summary>
    public DailyScheduleResponseDto TargetSchedule { get; set; } = null!;
}
