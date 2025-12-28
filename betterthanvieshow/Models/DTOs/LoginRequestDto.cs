using System.ComponentModel.DataAnnotations;

namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 會員登入請求 DTO
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// 電子信箱（登入帳號）
    /// </summary>
    /// <example>user@example.com</example>
    [Required(ErrorMessage = "信箱為必填欄位")]
    [EmailAddress(ErrorMessage = "信箱格式不正確")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    /// <example>Password123!</example>
    [Required(ErrorMessage = "密碼為必填欄位")]
    public string Password { get; set; } = string.Empty;
}
