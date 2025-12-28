namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影可訂票日期回應 DTO
/// </summary>
public class MovieAvailableDatesResponseDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    /// <example>1</example>
    public int MovieId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    /// <example>阿凡達：水之道</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 電影分級：
    /// - G（普遍級）
    /// - P（保護級）
    /// - PG（輔導級）
    /// - R（限制級）
    /// </summary>
    /// <example>G</example>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 電影時長（分鐘）
    /// </summary>
    /// <example>192</example>
    public int Duration { get; set; }

    /// <summary>
    /// 電影類型（多個以逗號分隔）：
    /// - Action (動作), Adventure (冒險), SciFi (科幻)
    /// </summary>
    /// <example>SciFi,Action,Adventure</example>
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
    /// <example>2023-10-25</example>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 星期幾（繁體中文，例如「週四」）
    /// </summary>
    /// <example>週三</example>
    public string DayOfWeek { get; set; } = string.Empty;
}
