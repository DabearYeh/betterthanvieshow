namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 影廳類型分組 DTO
/// </summary>
public class TheaterTypeGroupDto
{
    /// <summary>
    /// 影廳類型（Digital、4DX、IMAX）
    /// </summary>
    public string TheaterType { get; set; } = string.Empty;

    /// <summary>
    /// 影廳類型顯示名稱（數位、4DX、IMAX）
    /// </summary>
    public string TheaterTypeDisplay { get; set; } = string.Empty;

    /// <summary>
    /// 時間範圍（最早開始 - 最晚結束）
    /// </summary>
    public string TimeRange { get; set; } = string.Empty;

    /// <summary>
    /// 該影廳類型的所有場次
    /// </summary>
    public List<ShowtimeSimpleDto> Showtimes { get; set; } = new();
}
