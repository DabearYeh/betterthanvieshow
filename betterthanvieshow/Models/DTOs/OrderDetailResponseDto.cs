namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 訂單詳情回應 DTO
/// </summary>
public class OrderDetailResponseDto
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
    /// 訂單狀態
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 付款期限
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 電影資訊
    /// </summary>
    public OrderMovieInfoDto Movie { get; set; } = null!;

    /// <summary>
    /// 場次資訊
    /// </summary>
    public OrderShowtimeInfoDto Showtime { get; set; } = null!;

    /// <summary>
    /// 影廳資訊
    /// </summary>
    public OrderTheaterInfoDto Theater { get; set; } = null!;

    /// <summary>
    /// 座位列表
    /// </summary>
    public List<SeatInfoDto> Seats { get; set; } = new();

    /// <summary>
    /// 付款方式（如：Line Pay、未付款）
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// 應付總額
    /// </summary>
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// 訂單電影資訊 DTO
/// </summary>
public class OrderMovieInfoDto
{
    /// <summary>
    /// 電影標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 分級
    /// </summary>
    public string Rating { get; set; } = string.Empty;

    /// <summary>
    /// 片長（分鐘）
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// 海報 URL
    /// </summary>
    public string PosterUrl { get; set; } = string.Empty;
}

/// <summary>
/// 訂單場次資訊 DTO
/// </summary>
public class OrderShowtimeInfoDto
{
    /// <summary>
    /// 日期 (YYYY-MM-DD)
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間 (HH:mm)
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 星期幾
    /// </summary>
    public string DayOfWeek { get; set; } = string.Empty;
}

/// <summary>
/// 訂單影廳資訊 DTO
/// </summary>
public class OrderTheaterInfoDto
{
    /// <summary>
    /// 影廳名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
