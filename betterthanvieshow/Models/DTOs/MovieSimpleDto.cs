namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影簡化資訊 DTO（用於首頁列表展示）
/// </summary>
public class MovieSimpleDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 片名
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 海報 URL
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 時長（分鐘）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 影片類型
    /// </summary>
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// 電影分級
    /// </summary>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 上映日期
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// 下映日期
    /// </summary>
    public DateTime EndDate { get; set; }
}
