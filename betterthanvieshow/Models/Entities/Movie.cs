using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace betterthanvieshow.Models.Entities;

/// <summary>
/// 電影實體
/// </summary>
[Table("Movie")]
public class Movie
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 片名
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 簡介
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 時長（分鐘），必須 > 0
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "時長必須大於 0")]
    public int Duration { get; set; }

    /// <summary>
    /// 影片類型（多個用逗號分隔）
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Genre { get; set; } = string.Empty;

    /// <summary>
    /// 電影分級：普遍級、輔導級、限制級
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 導演
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Director { get; set; } = string.Empty;

    /// <summary>
    /// 演員
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Cast { get; set; } = string.Empty;

    /// <summary>
    /// 海報 URL
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 預告片連結
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string TrailerUrl { get; set; } = string.Empty;

    /// <summary>
    /// 電影上映日期
    /// </summary>
    [Required]
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// 電影下映日期
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 是否加入輪播
    /// </summary>
    public bool CanCarousel { get; set; } = false;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
