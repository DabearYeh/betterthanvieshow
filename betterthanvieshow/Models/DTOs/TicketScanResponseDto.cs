namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 掃描票券回應 DTO
/// </summary>
public class TicketScanResponseDto
{
    /// <summary>
    /// 票券 ID
    /// </summary>
    public int TicketId { get; set; }

    /// <summary>
    /// 票券編號
    /// </summary>
    public string TicketNumber { get; set; } = string.Empty;

    /// <summary>
    /// 票券狀態 (Pending, Unused, Used, Expired)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 電影海報 URL
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;

    /// <summary>
    /// 場次日期（格式：2025-12-31）
    /// </summary>
    public string ShowDate { get; set; } = string.Empty;

    /// <summary>
    /// 場次時間（格式：14:30）
    /// </summary>
    public string ShowTime { get; set; } = string.Empty;

    /// <summary>
    /// 座位排名（例如：D）
    /// </summary>
    public string SeatRow { get; set; } = string.Empty;

    /// <summary>
    /// 座位欄號（例如：12）
    /// </summary>
    public int SeatColumn { get; set; }

    /// <summary>
    /// 座位標籤（例如：D 排 12 號）
    /// </summary>
    public string SeatLabel { get; set; } = string.Empty;

    /// <summary>
    /// 影廳名稱（例如：2A）
    /// </summary>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型（Digital, IMAX, 4DX）
    /// </summary>
    public string TheaterType { get; set; } = string.Empty;
}
