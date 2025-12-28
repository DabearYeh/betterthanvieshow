namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 影廳詳細資訊回應 DTO（包含座位表）
/// </summary>
public class TheaterDetailResponseDto
{
    /// <summary>
    /// 影廳 ID
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// 影廳名稱
    /// </summary>
    /// <example>1廳</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 排數
    /// </summary>
    /// <example>10</example>
    public int RowCount { get; set; }

    /// <summary>
    /// 列數
    /// </summary>
    /// <example>15</example>
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
    /// <example>A</example>
    public string RowName { get; set; } = string.Empty;

    /// <summary>
    /// 欄號（1, 2, 3...）
    /// </summary>
    /// <example>1</example>
    public int ColumnNumber { get; set; }

    /// <summary>
    /// 座位類型（一般座位、殘障座位、走道、Empty）
    /// </summary>
    /// <example>Standard</example>
    public string SeatType { get; set; } = string.Empty;
}
