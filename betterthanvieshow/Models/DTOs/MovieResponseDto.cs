namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影回應 DTO
/// </summary>
public class MovieResponseDto
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
    /// 簡介
    /// </summary>
    public string Description { get; set; } = string.Empty;

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
    /// 導演
    /// </summary>
    public string Director { get; set; } = string.Empty;

    /// <summary>
    /// 演員
    /// </summary>
    public string Cast { get; set; } = string.Empty;

    /// <summary>
    /// 海報 URL
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 預告片連結
    /// </summary>
    public string TrailerUrl { get; set; } = string.Empty;

    /// <summary>
    /// 上映日期
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// 下映日期
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 是否加入輪播
    /// </summary>
    public bool CanCarousel { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
