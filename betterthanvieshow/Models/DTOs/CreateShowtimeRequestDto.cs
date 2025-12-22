using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 新增場次請求 DTO
/// </summary>
public class CreateShowtimeRequestDto
{
    /// <summary>
    /// 電影 ID
    /// </summary>
    [Required(ErrorMessage = "電影 ID 為必填")]
    public int MovieId { get; set; }

    /// <summary>
    /// 影廳 ID
    /// </summary>
    [Required(ErrorMessage = "影廳 ID 為必填")]
    public int TheaterId { get; set; }

    /// <summary>
    /// 放映日期，格式 YYYY-MM-DD
    /// </summary>
    [Required(ErrorMessage = "放映日期為必填")]
    public DateTime ShowDate { get; set; }

    /// <summary>
    /// 開始時間，格式 HH:MM
    /// 必須是 15 分鐘的倍數（如 09:00, 09:15, 09:30, 09:45）
    /// </summary>
    [Required(ErrorMessage = "開始時間為必填")]
    [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "時間格式必須為 HH:MM")]
    public string StartTime { get; set; } = string.Empty;
}
