namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 票卷詳細資訊 DTO
/// </summary>
public class TicketDetailDto
{
    /// <summary>
    /// 票卷 ID
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 票卷編號
    /// </summary>
    public string TicketNumber { get; set; } = string.Empty;

    /// <summary>
    /// 座位 ID
    /// </summary>
    public int SeatId { get; set; }

    /// <summary>
    /// 排名
    /// </summary>
    public string RowName { get; set; } = string.Empty;

    /// <summary>
    /// 欄號
    /// </summary>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// 票卷狀態 (Pending, Unused, Used, Expired)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// QR Code 內容 (前端用於生成 QR Code 圖片的字串)
    /// </summary>
    public string QrCodeContent { get; set; } = string.Empty;
}
