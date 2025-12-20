namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 會員登入回應 DTO
/// </summary>
public class LoginResponseDto
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
    /// JWT 認證 Token
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
