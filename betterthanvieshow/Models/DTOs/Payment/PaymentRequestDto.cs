using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs.Payment;

/// <summary>
/// 前端發起付款請求的 DTO
/// </summary>
public class PaymentRequestDto
{
    /// <summary>
    /// 訂單 ID
    /// </summary>
    [Required(ErrorMessage = "訂單 ID 為必填")]
    public int OrderId { get; set; }
}

/// <summary>
/// 後端回傳付款請求結果的 DTO
/// </summary>
public class PaymentRequestResponseDto
{
    /// <summary>
    /// LINE Pay 付款頁面網址 (使用者需跳轉至此)
    /// </summary>
    [Required]
    public string PaymentUrl { get; set; } = string.Empty;

    /// <summary>
    /// LINE Pay 交易 ID (19 位數字)
    /// </summary>
    [Required]
    public long TransactionId { get; set; }
}
