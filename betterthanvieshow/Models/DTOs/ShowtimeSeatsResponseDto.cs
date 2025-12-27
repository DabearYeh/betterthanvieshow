namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 場次座位配置回應 DTO
/// </summary>
public class ShowtimeSeatsResponseDto
{
    /// <summary>
    /// 場次 ID
    /// </summary>
    public int ShowTimeId { get; set; }

    /// <summary>
    /// 電影名稱
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// 放映日期（格式：YYYY-MM-DD）
    /// </summary>
    public string ShowDate { get; set; } = string.Empty;

    /// <summary>
    /// 開始時間（格式：HH:mm）
    /// </summary>
    public string StartTime { get; set; } = string.Empty;

    /// <summary>
    /// 結束時間（格式：HH:mm）
    /// </summary>
    public string EndTime { get; set; } = string.Empty;

    /// <summary>
    /// 影廳名稱
    /// </summary>
    public string TheaterName { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型
    /// </summary>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 票價
    /// </summary>
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
    public int SeatId { get; set; }

    /// <summary>
    /// 排名（如：A、B、C）
    /// </summary>
    public string RowName { get; set; } = string.Empty;

    /// <summary>
    /// 欄號
    /// </summary>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// 座位類型（一般座位、殘障座位、走道、Empty）
    /// </summary>
    public string SeatType { get; set; } = string.Empty;

    /// <summary>
    /// 座位狀態（available、sold、aisle、empty、invalid）
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid { get; set; }
}
