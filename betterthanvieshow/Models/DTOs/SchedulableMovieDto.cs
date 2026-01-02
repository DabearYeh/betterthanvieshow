namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 可排程電影 DTO（後台排程列表用）
/// </summary>
public class SchedulableMovieDto
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
}
