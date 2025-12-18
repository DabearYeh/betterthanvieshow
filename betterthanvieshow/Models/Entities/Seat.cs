using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace betterthanvieshow.Models.Entities;

/// <summary>
/// 座位實體
/// </summary>
[Table("Seat")]
public class Seat
{
    /// <summary>
    /// 座位 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 所屬影廳 ID
    /// </summary>
    [Required]
    public int TheaterId { get; set; }

    /// <summary>
    /// 座位排名（例如：A、B、C）
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string RowName { get; set; } = string.Empty;

    /// <summary>
    /// 欄號（正整數，一般從1開始）
    /// </summary>
    [Required]
    public int ColumnNumber { get; set; }

    /// <summary>
    /// 座位類型：一般座位、殘障座位、走道、Empty
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string SeatType { get; set; } = string.Empty;

    /// <summary>
    /// 座位是否有效
    /// </summary>
    [Required]
    public bool IsValid { get; set; } = true;

    /// <summary>
    /// 所屬影廳（導航屬性）
    /// </summary>
    public Theater? Theater { get; set; }
}
