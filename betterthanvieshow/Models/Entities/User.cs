using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace betterthanvieshow.Models.Entities;

/// <summary>
/// 使用者實體
/// </summary>
[Table("User")]
public class User
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// 登入帳號（信箱），必須唯一
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 使用者密碼（已加密）
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 使用者名稱
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色（英文枚舉值）：Customer（顧客）、Admin（管理者）
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Customer";

    /// <summary>
    /// 帳號建立時間
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
