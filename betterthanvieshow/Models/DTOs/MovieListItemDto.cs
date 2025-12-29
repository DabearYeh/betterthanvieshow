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
    /// 電影分級：
    /// - G（普遍級）
    /// - P（保護級）
    /// - PG（輔導級）
    /// - R（限制級）
    /// </summary>
    /// <example>G</example>
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
    /// 上映狀態：
    /// - ComingSoon（即將上映）
    /// - NowShowing（上映中）
    /// - OffScreen（已下映）
    /// </summary>
    /// <example>NowShowing</example>
    public string Status { get; set; } = string.Empty;
}
