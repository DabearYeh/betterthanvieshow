using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace betterthanvieshow.Models.Entities;

/// <summary>
/// 每日時刻表實體
/// 管理特定日期的場次排片狀態
/// </summary>
[Table("DailySchedule")]
public class DailySchedule
{
    /// <summary>
    /// 每日時刻表 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 日期，必須唯一
    /// </summary>
    [Required]
    public DateTime ScheduleDate { get; set; }

    /// <summary>
    /// 狀態：Draft（草稿）、OnSale（販售中）
    /// Draft 狀態下可新增/編輯場次；OnSale 狀態下該日期的場次無法編輯
    /// OnSale 狀態絕對不可逆轉回 Draft
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最後更新時間
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
