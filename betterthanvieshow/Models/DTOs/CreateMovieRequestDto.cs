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
    [Required(ErrorMessage = "片名為必填")]
    [MaxLength(200, ErrorMessage = "片名長度不可超過 200 字元")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 簡介
    /// </summary>
    [Required(ErrorMessage = "簡介為必填")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 時長（分鐘），必須 > 0
    /// </summary>
    [Required(ErrorMessage = "時長為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "時長必須大於 0")]
    public int Duration { get; set; }

    /// <summary>
    /// 影片類型（多個用逗號分隔）
    /// </summary>
    [Required(ErrorMessage = "影片類型為必填")]
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// 電影分級：普遍級、輔導級、限制級
    /// </summary>
    [Required(ErrorMessage = "電影分級為必填")]
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 導演
    /// </summary>
    [Required(ErrorMessage = "導演為必填")]
    public string Director { get; set; } = string.Empty;

    /// <summary>
    /// 演員
    /// </summary>
    [Required(ErrorMessage = "演員為必填")]
    public string Cast { get; set; } = string.Empty;

    /// <summary>
    /// 海報 URL
    /// </summary>
    [Required(ErrorMessage = "海報 URL 為必填")]
    [Url(ErrorMessage = "海報 URL 格式不正確")]
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 預告片連結
    /// </summary>
    [Required(ErrorMessage = "預告片連結為必填")]
    [Url(ErrorMessage = "預告片連結格式不正確")]
    public string TrailerUrl { get; set; } = string.Empty;

    /// <summary>
    /// 電影上映日期
    /// </summary>
    [Required(ErrorMessage = "上映日期為必填")]
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// 電影下映日期
    /// </summary>
    [Required(ErrorMessage = "下映日期為必填")]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 是否加入輪播
    /// </summary>
    public bool CanCarousel { get; set; } = false;
}
