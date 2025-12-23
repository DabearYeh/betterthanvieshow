using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace betterthanvieshow.Models.Entities;

/// <summary>
/// 場次實體
/// 電影排片的具體時段
/// </summary>
[Table("MovieShowTime")]
public class MovieShowTime
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 電影 ID
    /// </summary>
    [Required]
    public int MovieId { get; set; }

    /// <summary>
    /// 影廳 ID
    /// </summary>
    [Required]
    public int TheaterId { get; set; }

    /// <summary>
    /// 放映日期
    /// 規則：必須在 [Movie.ReleaseDate, Movie.EndDate] 範圍內
    /// </summary>
    [Required]
    public DateTime ShowDate { get; set; }

    /// <summary>
    /// 開始時間
    /// 規則：必須是 15 分鐘的倍數（如 09:00, 09:15, 09:30, 09:45）
    /// </summary>
    [Required]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// 關聯的電影
    /// </summary>
    [ForeignKey("MovieId")]
    public virtual Movie Movie { get; set; } = null!;

    /// <summary>
    /// 關聯的影廳
    /// </summary>
    [ForeignKey("TheaterId")]
    public virtual Theater Theater { get; set; } = null!;
}
