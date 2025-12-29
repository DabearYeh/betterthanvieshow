using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs.Payment;

/// <summary>
/// 前端確認付款的 DTO
/// </summary>
public class PaymentConfirmRequestDto
{
    /// <summary>
    /// LINE Pay 交易 ID (從 ConfirmUrl 參數取得)
    /// </summary>
    [Required(ErrorMessage = "交易 ID 為必填")]
    public long TransactionId { get; set; }

    /// <summary>
    /// 訂單 ID
    /// </summary>
    [Required(ErrorMessage = "訂單 ID 為必填")]
    public int OrderId { get; set; }
}

/// <summary>
/// 後端回傳付款確認結果的 DTO
/// </summary>
public class PaymentConfirmResponseDto
{
    /// <summary>
    /// 付款是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 訂單編號 (使用者友善格式，例如 #ABC-12345)
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// 票券列表 (含 QR Code)
    /// </summary>
    public List<TicketDto> Tickets { get; set; } = new();
}

/// <summary>
/// 票券資訊 DTO
/// </summary>
public class TicketDto
{
    /// <summary>
    /// 票券編號
    /// </summary>
    public string TicketNumber { get; set; } = string.Empty;

    /// <summary>
    /// 票券 QR Code (Base64 或 URL)
    /// </summary>
    public string QrCode { get; set; } = string.Empty;

    /// <summary>
    /// 座位資訊 (例如："H12")
    /// </summary>
    public string SeatInfo { get; set; } = string.Empty;
}
