namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 影廳詳細資訊回應 DTO（包含座位表）
/// </summary>
public class TheaterDetailResponseDto
{
    /// <summary>
    /// 影廳 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 排數
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// 列數
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// 座位表（二維陣列）
    /// </summary>
    public List<List<SeatDto>> SeatMap { get; set; } = new();
}

/// <summary>
/// 座位資訊 DTO
/// </summary>
public class SeatDto
{
    /// <summary>
    /// 排名（A, B, C...）
    /// </summary>
    public string RowName { get; set; } = string.Empty;

    /// <summary>
    /// 欄號（1, 2, 3...）
    /// </summary>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// 座位類型（一般座位、殘障座位、走道、Empty）
    /// </summary>
    public string SeatType { get; set; } = string.Empty;
}
