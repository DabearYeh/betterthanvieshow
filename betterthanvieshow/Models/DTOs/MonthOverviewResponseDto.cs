namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 月曆概覽回應 DTO
/// </summary>
public class MonthOverviewResponseDto
{
    /// <summary>
    /// 年份
    /// </summary>
    /// <example>2025</example>
    public int Year { get; set; }

    /// <summary>
    /// 月份
    /// </summary>
    /// <example>12</example>
    public int Month { get; set; }

    /// <summary>
    /// 該月份中有時刻表的日期清單
    /// </summary>
    public List<DailyScheduleStatusDto> Dates { get; set; } = new();
}

/// <summary>
/// 每日時刻表狀態 DTO
/// </summary>
public class DailyScheduleStatusDto
{
    /// <summary>
    /// 日期（格式：YYYY-MM-DD）
    /// </summary>
    /// <example>2025-12-01</example>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 狀態：Draft（草稿）或 OnSale（販售中）
    /// </summary>
    /// <example>OnSale</example>
    public string Status { get; set; } = string.Empty;
}
