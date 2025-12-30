using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace betterthanvieshow.Models.Entities;

/// <summary>
/// 驗票記錄實體
/// 記錄票券驗證的時間與人員
/// </summary>
[Table("TicketValidateLog")]
public class TicketValidateLog
{
    /// <summary>
    /// 驗票記錄 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 票券 ID
    /// </summary>
    [Required]
    public int TicketId { get; set; }

    /// <summary>
    /// 驗票時間
    /// </summary>
    [Required]
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 驗票人員 ID
    /// </summary>
    [Required]
    public int ValidatedBy { get; set; }

    /// <summary>
    /// 驗票結果：true（有效）、false（無效）
    /// </summary>
    [Required]
    public bool ValidationResult { get; set; }

    // Navigation Properties

    /// <summary>
    /// 關聯的票券
    /// </summary>
    [ForeignKey("TicketId")]
    public virtual Ticket Ticket { get; set; } = null!;

    /// <summary>
    /// 驗票人員（管理者）
    /// </summary>
    [ForeignKey("ValidatedBy")]
    public virtual User Validator { get; set; } = null!;
}
