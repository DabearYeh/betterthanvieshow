namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影列表項目 DTO
/// </summary>
public class MovieListItemDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// 片名
    /// </summary>
    /// <example>蜘蛛人：穿越新宇宙</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 海報 URL
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 時長（分鐘）
    /// </summary>
    /// <example>140</example>
    public int Duration { get; set; }

    /// <summary>
    /// 電影分級
    /// </summary>
    /// <example>普遍級</example>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 上映日期
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// 下映日期
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 上映狀態：即將上映、上映中、已下映
    /// </summary>
    /// <example>上映中</example>
    public string Status { get; set; } = string.Empty;
}
