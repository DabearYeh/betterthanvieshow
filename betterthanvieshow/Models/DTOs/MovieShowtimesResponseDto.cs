namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影場次列表回應 DTO
/// </summary>
public class MovieShowtimesResponseDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    public int MovieId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 查詢日期（格式：YYYY-MM-DD）
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 場次列表
    /// </summary>
    public List<ShowtimeListItemDto> Showtimes { get; set; } = new();
}

/// <summary>
/// 場次列表項目 DTO（用於前台場次列表顯示）
/// </summary>
public class ShowtimeListItemDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    public int ShowTimeId { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型（一般數位、4DX、IMAX）
    /// </summary>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間（格式：HH:mm）
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（格式：HH:mm，動態計算）
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// 票價（根據影廳類型）
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    /// 可用座位數
    /// </summary>
    public int AvailableSeats { get; set; }

    /// <summary>
    /// 總座位數
    /// </summary>
    public int TotalSeats { get; set; }
}
