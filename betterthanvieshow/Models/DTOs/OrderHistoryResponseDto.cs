namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 訂單歷史紀錄回應 DTO
/// </summary>
public class OrderHistoryResponseDto
{
    /// <summary>
    /// 訂單 ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 電影海報 URL
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 場次時間
    /// </summary>
    public DateTime ShowTime { get; set; }

    /// <summary>
    /// 票卷數量
    /// </summary>
    public int TicketCount { get; set; }

    /// <summary>
    /// 電影時長（分鐘）
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// 訂單狀態 (Pending, Paid, Cancelled)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 是否已使用（包含已過期、已驗票等）
    /// </summary>
    public bool IsUsed { get; set; }
}
