namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影簡化資訊 DTO（用於首頁列表展示）
/// </summary>
public class MovieSimpleDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// 片名
    /// </summary>
    /// <example>少林足球</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 海報 URL
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 時長（分鐘）
    /// </summary>
    /// <example>113</example>
    public int Duration { get; set; }

    /// <summary>
    /// 影片類型
    /// </summary>
    /// <example>喜劇, 動作</example>
    public string Genre { get; set; } = string.Empty;

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
}
