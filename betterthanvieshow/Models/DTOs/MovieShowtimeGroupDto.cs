namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影場次分組 DTO
/// </summary>
public class MovieShowtimeGroupDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    public int MovieId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 電影海報 URL
    /// </summary>
    public string? PosterUrl { get; set; }

    /// <summary>
    /// 電影分級（G、PG、R）
    /// </summary>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 電影分級顯示格式（0+、12+、18+）
    /// </summary>
    public string RatingDisplay { get; set; } = string.Empty;

    /// <summary>
    /// 電影時長（分鐘）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 片長顯示格式（如：2 小時 23 分鐘）
    /// </summary>
    public string DurationDisplay { get; set; } = string.Empty;

    /// <summary>
    /// 按影廳類型分組的場次
    /// </summary>
    public List<TheaterTypeGroupDto> TheaterTypeGroups { get; set; } = new();
}
