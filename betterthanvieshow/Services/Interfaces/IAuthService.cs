using betterthanvieshow.Models.DTOs;
using betterthanvieshow.Models.Responses;

namespace betterthanvieshow.Services.Interfaces;

/// <summary>
/// 認證服務介面
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// 會員註冊
    /// </summary>
    /// <param name="request">註冊請求</param>
    /// <returns>註冊結果</returns>
    Task<ApiResponse<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// 會員登入
    /// </summary>
    /// <param name="request">登入請求</param>
    /// <returns>登入結果</returns>
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
}
