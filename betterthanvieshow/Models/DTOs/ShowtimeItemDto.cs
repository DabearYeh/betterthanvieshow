using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 單一場次資料
/// </summary>
public class ShowtimeItemDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    /// <example>1</example>
    [Required(ErrorMessage = "電影 ID 為必填")]
    public int MovieId { get; set; }

    /// <summary>
    /// 影廳 ID
    /// </summary>
    /// <example>3</example>
    [Required(ErrorMessage = "影廳 ID 為必填")]
    public int TheaterId { get; set; }

    /// <summary>
    /// 開始時間，格式 HH:MM
    /// 必須是 15 分鐘的倍數（如 09:00, 09:15, 09:30, 09:45）
    /// </summary>
    /// <example>14:30</example>
    [Required(ErrorMessage = "開始時間為必填")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "時間格式必須為 HH:MM")]
    public string StartTime { get; set; } = string.Empty;
}
