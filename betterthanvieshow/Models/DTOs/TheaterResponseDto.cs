namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 影廳回應 DTO
/// </summary>
public class TheaterResponseDto
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
    /// 影廳類型
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 所在樓層
    /// </summary>
    public int Floor { get; set; }

    /// <summary>
    /// 排數
    /// </summary>
    public int RowCount { get; set; }

    /// <summary>
    /// 列數
    /// </summary>
    public int ColumnCount { get; set; }

    /// <summary>
    /// 座位總數
    /// </summary>
    public int TotalSeats { get; set; }
}
