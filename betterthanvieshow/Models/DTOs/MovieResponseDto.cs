namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影回應 DTO
/// </summary>
public class MovieResponseDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// 片名
    /// </summary>
    /// <example>全面啟動</example>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 簡介
    /// </summary>
    /// <example>造夢者唐姆柯比（李奧納多狄卡皮歐 飾）在本片中飾演一名神偷...</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 時長（分鐘）
    /// </summary>
    /// <example>148</example>
    public int Duration { get; set; }

    /// <summary>
    /// 影片類型：
    /// - Action (動作), Adventure (冒險), SciFi (科幻)
    /// </summary>
    /// <example>SciFi, Action</example>
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
    /// 導演
    /// </summary>
    /// <example>克里斯多福·諾蘭</example>
    public string Director { get; set; } = string.Empty;

    /// <summary>
    /// 演員
    /// </summary>
    /// <example>李奧納多·狄卡皮歐, 喬瑟夫·高登-李維</example>
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
    /// <example>false</example>
    public bool CanCarousel { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
