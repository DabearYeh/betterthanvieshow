namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 場次座位配置回應 DTO
/// </summary>
public class ShowtimeSeatsResponseDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    /// <example>100</example>
    public int ShowTimeId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    /// <example>阿凡達</example>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 放映日期（格式：YYYY-MM-DD）
    /// </summary>
    /// <example>2023-10-25</example>
    public string ShowDate { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間（格式：HH:mm）
    /// </summary>
    /// <example>10:00</example>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（格式：HH:mm）
    /// </summary>
    /// <example>13:12</example>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// 影廳名稱
    /// </summary>
    /// <example>IMAX廳</example>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型
    /// </summary>
    /// <example>IMAX</example>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 票價
    /// </summary>
    /// <example>380</example>
    public int Price { get; set; }

    /// <summary>
    /// 排數
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// 列數
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// 座位二維陣列
    /// </summary>
    public List<List<ShowtimeSeatDto>> Seats { get; set; } = new();
}

/// <summary>
/// 場次座位 DTO（用於座位選擇頁面）
/// </summary>
public class ShowtimeSeatDto
{
    /// <summary>
    /// 座位 ID（0 表示不存在的位置）
    /// </summary>
    /// <example>101</example>
    public int SeatId { get; set; }

    /// <summary>
    /// 排名（如：A、B、C）
    /// </summary>
    /// <example>A</example>
    public string RowName { get; set; } = string.Empty;

    /// <summary>
    /// 欄號
    /// </summary>
    /// <example>1</example>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// 座位類型（一般座位、殘障座位、走道、Empty）
    /// </summary>
    /// <example>Standard</example>
    public string SeatType { get; set; } = string.Empty;

    /// <summary>
    /// 座位狀態（available、sold、aisle、empty、invalid）
    /// </summary>
    /// <example>available</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 是否有效
    /// </summary>
    /// <example>true</example>
    public bool IsValid { get; set; }
}
