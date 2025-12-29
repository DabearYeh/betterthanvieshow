using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace betterthanvieshow.Models.Entities;

/// <summary>
/// 影廳實體
/// </summary>
[Table("Theater")]
public class Theater
{
    /// <summary>
    /// 影廳 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型：Digital（300元）、4DX（380元）、IMAX（380元）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 所在樓層
    /// </summary>
    [Required]
    public int Floor { get; set; }

    /// <summary>
    /// 排數，必須 > 0
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "排數必須大於 0")]
    public int RowCount { get; set; }

    /// <summary>
    /// 列數，必須 > 0
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "列數必須大於 0")]
    public int ColumnCount { get; set; }

    /// <summary>
    /// 座位總數，必須 > 0
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "座位總數必須大於 0")]
    public int TotalSeats { get; set; }

    /// <summary>
    /// 座位列表（導航屬性）
    /// </summary>
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}
