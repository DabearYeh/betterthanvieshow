namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影可訂票日期回應 DTO
/// </summary>
public class MovieAvailableDatesResponseDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    public int MovieId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 電影分級（普遍級、輔導級、限制級）
    /// </summary>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 電影時長（分鐘）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 電影類型（多個用逗號分隔）
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// 海報 URL
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 預告片 URL
    /// </summary>
    public string TrailerUrl { get; set; } = string.Empty;

    /// <summary>
    /// 可訂票日期列表
    /// </summary>
    public List<DateItemDto> Dates { get; set; } = new();
}

/// <summary>
/// 日期項目 DTO
/// </summary>
public class DateItemDto
{
    /// <summary>
    /// 日期（格式：YYYY-MM-DD）
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 星期幾（繁體中文，例如「週四」）
    /// </summary>
    public string DayOfWeek { get; set; } = string.Empty;
}
