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
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電子信箱
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 使用者角色
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// JWT 認證 Token（用於自動登入）
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 帳號建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
