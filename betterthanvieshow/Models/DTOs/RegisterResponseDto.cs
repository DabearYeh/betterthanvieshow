namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 會員註冊回應 DTO
/// </summary>
public class RegisterResponseDto
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    /// <example>王小明</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電子信箱
    /// </summary>
    /// <example>user@example.com</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 使用者角色
    /// </summary>
    /// <example>Member</example>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// JWT 認證 Token（用於自動登入）
    /// </summary>
    /// <example>eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...</example>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 帳號建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
