using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 創建訂單請求 DTO
/// </summary>
public class CreateOrderRequestDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    [Required(ErrorMessage = "場次 ID 為必填項目")]
    public int ShowTimeId { get; set; }

    /// <summary>
    /// 座位 ID 列表
    /// </summary>
    [Required(ErrorMessage = "座位 ID 列表為必填項目")]
    [MinLength(1, ErrorMessage = "至少需要選擇 1 個座位")]
    [MaxLength(6, ErrorMessage = "最多只能選擇 6 個座位")]
    public List<int> SeatIds { get; set; } = new();
}
