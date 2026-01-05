namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 影廳回應 DTO
/// </summary>
public class TheaterResponseDto
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
    /// 影廳類型
    /// </summary>
    /// <example>Digital</example>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 所在樓層
    /// </summary>
    /// <example>3</example>
    public int Floor { get; set; }

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
    /// 一般座位數
    /// </summary>
    /// <example>48</example>
    public int Standard { get; set; }

    /// <summary>
    /// 殘障座位數
    /// </summary>
    /// <example>4</example>
    public int Wheelchair { get; set; }

    /// <summary>
    /// 是否可以刪除（影廳沒有關聯的場次時為 true）
    /// </summary>
    /// <example>true</example>
    public bool CanDelete { get; set; }
}
