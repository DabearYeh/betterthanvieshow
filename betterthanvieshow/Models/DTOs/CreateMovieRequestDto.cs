using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 建立電影請求 DTO
/// </summary>
public class CreateMovieRequestDto
{
    /// <summary>
    /// 片名
    /// </summary>
    /// <example>阿凡達：水之道</example>
    [Required(ErrorMessage = "片名為必填")]
    [MaxLength(200, ErrorMessage = "片名長度不可超過 200 字元")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 簡介
    /// </summary>
    /// <example>傑克·蘇里與奈蒂莉在潘朵拉星球定居，為了保護家人，他們必須再次戰鬥...</example>
    [Required(ErrorMessage = "簡介為必填")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 時長（分鐘），必須 > 0
    /// </summary>
    /// <example>192</example>
    [Required(ErrorMessage = "時長為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "時長必須大於 0")]
    public int Duration { get; set; }

    /// <summary>
    /// 影片類型（多個以逗號分隔，請輸入英文代碼）：
    /// - Action (動作), Adventure (冒險), SciFi (科幻)
    /// - Comedy (喜劇), Drama (劇情), Horror (恐怖)
    /// - Animation (動畫), Romance (愛情), Thriller (驚悚)
    /// </summary>
    /// <example>SciFi,Action,Adventure</example>
    [Required(ErrorMessage = "影片類型為必填")]
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// 電影分級（請輸入英文代碼）：
    /// - G（普遍級）
    /// - P（保護級）
    /// - PG（輔導級）
    /// - R（限制級）
    /// </summary>
    /// <example>G</example>
    [Required(ErrorMessage = "電影分級為必填")]
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 導演
    /// </summary>
    /// <example>詹姆斯·卡麥隆</example>
    [Required(ErrorMessage = "導演為必填")]
    public string Director { get; set; } = string.Empty;

    /// <summary>
    /// 演員
    /// </summary>
    /// <example>山姆·沃辛頓, 柔伊·莎達娜</example>
    [Required(ErrorMessage = "演員為必填")]
    public string Cast { get; set; } = string.Empty;

    /// <summary>
    /// 海報 URL
    /// </summary>
    /// <example>https://example.com/poster.jpg</example>
    [Required(ErrorMessage = "海報 URL 為必填")]
    [Url(ErrorMessage = "海報 URL 格式不正確")]
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 預告片連結
    /// </summary>
    /// <example>https://youtube.com/watch?v=123</example>
    [Required(ErrorMessage = "預告片連結為必填")]
    [Url(ErrorMessage = "預告片連結格式不正確")]
    public string TrailerUrl { get; set; } = string.Empty;

    /// <summary>
    /// 電影上映日期
    /// </summary>
    /// <example>2023-12-15T00:00:00</example>
    [Required(ErrorMessage = "上映日期為必填")]
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// 電影下映日期
    /// </summary>
    /// <example>2024-03-15T00:00:00</example>
    [Required(ErrorMessage = "下映日期為必填")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 是否加入輪播
    /// </summary>
    /// <example>true</example>
    public bool CanCarousel { get; set; } = false;
}
