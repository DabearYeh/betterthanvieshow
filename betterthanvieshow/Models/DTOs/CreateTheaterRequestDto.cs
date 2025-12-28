using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 建立影廳請求 DTO
/// </summary>
public class CreateTheaterRequestDto
{
    /// <summary>
    /// 影廳名稱
    /// </summary>
    /// <example>IMAX廳</example>
    [Required(ErrorMessage = "影廳名稱為必填")]
    [MaxLength(100, ErrorMessage = "影廳名稱長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型：一般數位、4DX、IMAX
    /// </summary>
    /// <example>IMAX</example>
    [Required(ErrorMessage = "影廳類型為必填")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 所在樓層
    /// </summary>
    /// <example>5</example>
    [Required(ErrorMessage = "樓層為必填")]
    public int Floor { get; set; }

    /// <summary>
    /// 排數，必須 > 0
    /// </summary>
    /// <example>15</example>
    [Required(ErrorMessage = "排數為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "排數必須大於 0")]
    public int RowCount { get; set; }

    /// <summary>
    /// 列數，必須 > 0
    /// </summary>
    /// <example>20</example>
    [Required(ErrorMessage = "列數為必填")]
    [Range(1, int.MaxValue, ErrorMessage = "列數必須大於 0")]
    public int ColumnCount { get; set; }

    /// <summary>
    /// 座位配置（二維陣列）
    /// 每個元素代表座位類型（需傳入英文）：
    /// - Standard (一般座位)
    /// - Wheelchair (殘障座位)
    /// - Aisle (走道)
    /// - Empty (空位)
    /// </summary>
    /// <example>[["Standard", "Aisle", "Standard"], ["Wheelchair", "Aisle", "Standard"]]</example>
    [Required(ErrorMessage = "座位配置為必填")]
    public List<List<string>> Seats { get; set; } = new();
}
