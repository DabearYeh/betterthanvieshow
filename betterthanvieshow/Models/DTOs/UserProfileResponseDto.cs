namespace betterthanvieshow.Models.DTOs;

/// <summary>
/// 使用者個人資料回應 DTO
/// </summary>
public class UserProfileResponseDto
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 電子信箱
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
