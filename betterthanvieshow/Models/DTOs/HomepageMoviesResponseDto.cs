namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 首頁電影資料回應 DTO
/// </summary>
public class HomepageMoviesResponseDto
{
    /// <summary>
    /// 輪播圖電影（CanCarousel = true 且未下映）
    /// </summary>
    public List<MovieSimpleDto> Carousel { get; set; } = new();

    /// <summary>
    /// 本週前10（根據票券銷售數量排序）
    /// </summary>
    public List<MovieSimpleDto> TopWeekly { get; set; } = new();

    /// <summary>
    /// 即將上映（ReleaseDate > 今天）
    /// </summary>
    public List<MovieSimpleDto> ComingSoon { get; set; } = new();

    /// <summary>
    /// 隨機推薦
    /// </summary>
    public List<MovieSimpleDto> Recommended { get; set; } = new();

    /// <summary>
    /// 所有電影（正在上映 + 即將上映）
    /// </summary>
    public List<MovieSimpleDto> AllMovies { get; set; } = new();
}
