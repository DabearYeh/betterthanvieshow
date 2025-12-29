using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 複製時刻表請求 DTO
/// </summary>
public class CopyDailyScheduleRequestDto
{
    /// <summary>
    /// 目標日期（複製到此日期），格式：YYYY-MM-DD
    /// </summary>
    /// <example>2025-12-25</example>
    [Required(ErrorMessage = "目標日期為必填")]
    public string TargetDate { get; set; } = string.Empty;
}
