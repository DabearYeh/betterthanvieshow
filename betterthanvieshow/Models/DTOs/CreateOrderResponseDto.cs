namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 創建訂單回應 DTO
/// </summary>
public class CreateOrderResponseDto
{
    /// <summary>
    /// 訂單 ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// 訂單編號
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// 訂單總金額
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// 付款期限
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 票券數量
    /// </summary>
    public int TicketCount { get; set; }

    /// <summary>
    /// 座位資訊列表
    /// </summary>
    public List<SeatInfoDto> Seats { get; set; } = new();
}

/// <summary>
/// 座位資訊 DTO
/// </summary>
public class SeatInfoDto
{
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
    /// 票券編號
    /// </summary>
    public string TicketNumber { get; set; } = string.Empty;
}
