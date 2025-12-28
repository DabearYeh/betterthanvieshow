using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 會員註冊請求 DTO
/// </summary>
public class RegisterRequestDto
{
    /// <summary>
    /// 使用者名稱
    /// </summary>
    /// <example>王小明</example>
    [Required(ErrorMessage = "名稱為必填欄位")]
    [StringLength(100, ErrorMessage = "名稱長度不可超過 100 字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電子信箱（登入帳號）
    /// </summary>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "信箱為必填欄位")]
    [EmailAddress(ErrorMessage = "信箱格式不正確")]
    [StringLength(255, ErrorMessage = "信箱長度不可超過 255 字元")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    /// <example>Password123!</example>
    [Required(ErrorMessage = "密碼為必填欄位")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "密碼至少需 8 字元，包含大小寫字母與數字")]
    public string Password { get; set; } = string.Empty;
}
