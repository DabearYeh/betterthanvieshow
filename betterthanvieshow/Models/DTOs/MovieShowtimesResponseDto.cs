namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 電影場次列表回應 DTO
/// </summary>
public class MovieShowtimesResponseDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    /// <example>1</example>
    public int MovieId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    /// <example>阿凡達：水之道</example>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 查詢日期（格式：YYYY-MM-DD）
    /// </summary>
    /// <example>2023-10-25</example>
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
    /// <example>100</example>
    public int ShowTimeId { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    /// <example>IMAX廳</example>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型（Digital、4DX、IMAX）
    /// </summary>
    /// <example>IMAX</example>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間（格式：HH:mm）
    /// </summary>
    /// <example>14:00</example>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（格式：HH:mm，動態計算）
    /// </summary>
    /// <example>17:12</example>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// 票價（根據影廳類型）
    /// </summary>
    /// <example>380</example>
    public int Price { get; set; }

    /// <summary>
    /// 可用座位數
    /// </summary>
    /// <example>150</example>
    public int AvailableSeats { get; set; }

    /// <summary>
    /// 總座位數
    /// </summary>
    /// <example>200</example>
    public int TotalSeats { get; set; }
}
