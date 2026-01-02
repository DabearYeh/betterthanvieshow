using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 使用者服務介面
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 取得使用者個人資料
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <returns>使用者個人資料</returns>
    Task<ApiResponse<UserProfileResponseDto>> GetUserProfileAsync(int userId);
}
